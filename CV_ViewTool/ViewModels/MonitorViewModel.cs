using ColorPicker.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CV_ViewTool.Contracts.Services;
using CV_ViewTool.Contracts.ViewModels;
using CV_ViewTool.Converters;
using CV_ViewTool.Helpers;
using CV_ViewTool.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Newtonsoft.Json;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CV_ViewTool.ViewModels;

public class MonitorViewModel : ObservableObject, INavigationAware
{
    public readonly record struct PercentageRecord(DateTime DateTime, double Percentage);

    private bool _enableImage = true;
    private string _profileName = string.Empty;
    private bool _useInRange;
    private BitmapImage _originalImage;
    private BitmapImage _processedImage;
    private BitmapImage _sampleImage;
    private int _threshold = 125;
    private int _maxBinary = 255;
    private int _fps = 0;
    private int _modeIndex = 0;
    /// <summary>
    /// 预期计算结果（白色占比/色彩直方图计算结果/模板匹配预期）
    /// </summary>
    private double _whiteRatio = 25;
    /// <summary>
    /// 容差
    /// </summary>
    private double _tolerance = 6;
    /// <summary>
    /// 连续匹配的阈值
    /// </summary>
    private int _consecutiveMatchesThreshold = 0;
    private ColorState _colorState = new();
    private ColorState _colorState2 = new();
    private ICommand _showImageCommand;
    private ICommand _setSampleImageCommand;
    private ICommand _setCameraCommand;
    private ICommand _initCameraListCommand;

    public bool EnableImage { get => _enableImage; set => SetProperty(ref _enableImage, value); }
    public string ProfileName { get => _profileName; set => SetProperty(ref _profileName, value); }
    public bool UseInRange { get => _useInRange; set => SetProperty(ref _useInRange, value); }
    public BitmapImage OriginalImage { get => _originalImage; set => SetProperty(ref _originalImage, value); }
    public BitmapImage ProcessedImage { get => _processedImage; set => SetProperty(ref _processedImage, value); }
    public BitmapImage SampleImage { get => _sampleImage; set => SetProperty(ref _sampleImage, value); }
    public int Threshold { get => _threshold; set => SetProperty(ref _threshold, value); }
    public int MaxBinary { get => _maxBinary; set => SetProperty(ref _maxBinary, value); }
    public int FPS { get => _fps; set => SetProperty(ref _fps, value); }
    public int ModeIndex { get => _modeIndex; set => SetProperty(ref _modeIndex, value); }
    public double WhiteRatio
    {
        get => _whiteRatio; set
        {
            GreaterThanAndLessThanConverter.DefaultCompareValue = value;
            SetProperty(ref _whiteRatio, value);
        }
    }
    public double Tolerance
    {
        get => _tolerance; set
        {
            GreaterThanAndLessThanConverter.DefaultToleranceValue = value;
            SetProperty(ref _tolerance, value);
        }
    }
    public int ConsecutiveMatchesThreshold { get => _consecutiveMatchesThreshold; set => SetProperty(ref _consecutiveMatchesThreshold, value); }
    public ColorState ColorState { get => _colorState; set => SetProperty(ref _colorState, value); }
    public ColorState ColorState2 { get => _colorState2; set => SetProperty(ref _colorState2, value); }
    public ObservableCollection<string> CameraNameList { get; } = new();
    public ObservableCollection<PercentageRecord> ProbabilityList { get; } = new();

    public ICommand ShowImageCommand => _showImageCommand??=new RelayCommand(() => { EnableImage = !EnableImage; });
    public ICommand SetSampleImageCommand => _setSampleImageCommand??=new RelayCommand(SetSampleImage);
    public ICommand SetCameraCommand => _setCameraCommand??=new RelayCommand<int>(SetCamera);
    public ICommand InitCameraListCommand => _initCameraListCommand ??=new RelayCommand(InitCameraList);

    private Queue<Action> Queue { get; } = new();
    private object IsImageProcessing { get; } = new();
    private IAppState AppState { get; }
    private Mat FixedImage { get; set; } = new();
    private ScreenCaptureArea ScreenCaptureArea { get; set; } = new();
    private int ConsecutiveMatchesCount { get; set; } = 0;

    public MonitorViewModel(IServiceProvider service)
    {
        AppState = service.GetRequiredService<IAppState>();

        //Task.Run(async () =>
        //{
        //    while (true)
        //    {
        //        DateTime dateTime = DateTime.Now;
        //        double percentage = Random.Shared.NextDouble() * 100;
        //        App.Current.Dispatcher.Invoke(() =>
        //        {
        //            while (ProbabilityList.Count >= 10)
        //            {
        //                ProbabilityList.RemoveAt(ProbabilityList.Count-1);
        //            }
        //            ProbabilityList.Insert(0, new() { DateTime =  dateTime, Percentage = percentage });
        //        });

        //        await Task.Delay(1000).ConfigureAwait(false);
        //    }
        //});
    }

    public void OnNavigatedTo(object parameter)
    {
        if (parameter is not Dictionary<string, object> dic) return;

        if (dic.ContainsKey("ProfileName") && dic["ProfileName"] is string profileName)
        {
            ProfileName = profileName;
            string profileList = App.Current.Properties["ProfileList"]?.ToString() ?? "";
            List<CVProfile> profiles = JsonConvert.DeserializeObject<List<CVProfile>>(profileList) ?? new();
            if (profiles.FirstOrDefault(p => p.ProfileName.ToUpper() == ProfileName.ToUpper()) is not CVProfile profile)
                return;

            Threshold = profile.Threshold;
            MaxBinary = profile.MaxBinary;
            ColorState = profile.ColorState;
            ColorState2 = profile.ColorState2;
            UseInRange = profile.UseInRange;
        }

        if (dic.ContainsKey("ScreenShut") && dic["ScreenShut"] is Bitmap bitmap)
        {
            OriginalImage = bitmap.BitmapToBitmapImage();
            //Task.Run(() =>
            //{
            ImageProcessing(bitmap);
            //});
        }
        if (dic.ContainsKey("FPS") && dic["FPS"] is double fps)
        {
            FPS = (int)fps;
        }
        if (dic.ContainsKey("InitTouchArea") && dic["InitTouchArea"] is ScreenCaptureArea screenCaptureArea)
        {
            ScreenCaptureArea = screenCaptureArea;
        }
    }

    public void OnNavigatedFrom()
    {
        //throw new NotImplementedException();
    }

    private void SetSampleImage()
    {
        OpenFileDialog dlg = new()
        {
            // Set filter for file extension and default file extension 
            DefaultExt = ".png",
            Filter = "Image Files|*.jpeg;*.png;*.jpg|JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg"
        };

        // Display OpenFileDialog by calling ShowDialog method 
        bool? result = dlg.ShowDialog();

        // Get the selected file name and display in a TextBox 
        if (result != true) return;
        // Open document 
        string filename = dlg.FileName;

        Bitmap sampleBitmap = new(filename);
        SampleImage = sampleBitmap.BitmapToBitmapImage();

        // 读取图像
        Mat image = Cv2.ImRead(filename, ImreadModes.AnyColor);

        // 将图像转换为灰度图像
        Cv2.CvtColor(image, FixedImage, ColorConversionCodes.BGR2GRAY);
        // 释放图像
        image.Release();
    }

    private void SetCamera(int cameraIndex)
    {
        if (cameraIndex < 0 || cameraIndex >= CameraNameList.Count) return;
        string name = CameraNameList[cameraIndex];
        string monitorPageId = "MonitorWindow_" + ProfileName;
        if (AppState.CameraMonitorState.ContainsKey(name))
        {
            foreach (string key in AppState.CameraMonitorState.Keys)
            {
                AppState.CameraMonitorState[key].Remove(monitorPageId);
            }

            if (!AppState.CameraMonitorState[name].Contains(monitorPageId))
                AppState.CameraMonitorState[name].Add(monitorPageId);
        }
        else
        {
            AppState.CameraMonitorState.TryAdd(name, new() { monitorPageId });
        }
    }

    private void InitCameraList()
    {
        CameraNameList.Clear();
        foreach (string key in AppState.CameraState.Keys)
        {
            CameraNameList.Add(key);
        }
    }

    private void ImageProcessing(Bitmap bitmap)
    {
        if (!EnableImage || bitmap is null) return;

        Queue.Enqueue((() =>
        {
            try
            {
                Bitmap bitmap2 = null;
                using Mat image = BitmapConverter.ToMat(bitmap);
                if (!UseInRange)
                {
                    Cv2.CvtColor(image, image, ColorConversionCodes.BGR2GRAY);
                    Cv2.Threshold(image, image, Threshold, MaxBinary, ThresholdTypes.Binary);
                    bitmap2 = BitmapConverter.ToBitmap(image);
                }
                else
                {
                    Cv2.CvtColor(image, image, ColorConversionCodes.BGR2HSV);
                    Cv2.InRange(
                            image,
                            new Scalar(ColorState.HSV_H / 2, ColorState.HSV_S * 255, ColorState.HSV_V * 255),
                            new Scalar(ColorState2.HSV_H / 2, ColorState2.HSV_S * 255, ColorState2.HSV_V * 255),
                            image);
                    bitmap2 = BitmapConverter.ToBitmap(image);
                }

                if (bitmap2 is null) return;

                ProcessedImage = bitmap2.BitmapToBitmapImage();

                DateTime dateTime = DateTime.Now;
                double percentage = 0;

                if (ModeIndex==0)
                {
                    percentage = CalculateWhiteRatio(bitmap2);
                }
                else if (ModeIndex==1 && SampleImage is not null)
                {
                    percentage = CalculateSimilarity(FixedImage, bitmap2);
                }
                else if (ModeIndex==2 && SampleImage is not null)
                {
                    percentage = CalculateSimilarityByHist(FixedImage, bitmap2);
                }

                App.Current.Dispatcher.Invoke(() =>
                {
                    while (ProbabilityList.Count >= 10)
                    {
                        ProbabilityList.RemoveAt(ProbabilityList.Count - 1);
                    }

                    ProbabilityList.Insert(0, new(dateTime, percentage));
                });

                if (percentage > WhiteRatio - Tolerance && percentage < WhiteRatio +  Tolerance)
                {
                    ConsecutiveMatchesCount++;
                    if (ConsecutiveMatchesCount >= ConsecutiveMatchesThreshold)
                    {
                        Nukepayload2.Diagnostics.Interaction.SendTouch(
                        posX: ScreenCaptureArea.Left + (ScreenCaptureArea.Width / 2),
                        posY: ScreenCaptureArea.Top + (ScreenCaptureArea.Height / 2),
                        durationMilliseconds: 20);
                        ConsecutiveMatchesCount = 0;
                    }
                }
                else
                {
                    ConsecutiveMatchesCount = 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }));
        RunImageProcessingQueue();
    }

    private void RunImageProcessingQueue()
    {
        lock (IsImageProcessing)
        {
            while (Queue.Count > 0)
            {
                Queue.Dequeue().Invoke();
            }
        }
    }

    private static double CalculateWhiteRatio(Bitmap liveImage)
    {
        // 读取黑白图像
        Mat image = liveImage.ToMat();

        // 统计白色像素数量
        int whitePixelCount = 0;
        int totalPixelCount = image.Rows * image.Cols;

        for (int y = 0; y < image.Rows; y++)
        {
            for (int x = 0; x < image.Cols; x++)
            {
                Vec3b pixelValue = image.Get<Vec3b>(y, x);
                if (pixelValue.Item0 == 255) // 假设255表示白色
                {
                    whitePixelCount++;
                }
            }
        }

        // 计算白色占比
        double whiteRatio = (double)whitePixelCount / totalPixelCount;

        return whiteRatio;
    }

    private static double CalculateSimilarity(Mat fixedImage, Bitmap liveImage)
    {
        Mat liveImageMat = liveImage.ToMat();

        // 进行模板匹配
        Mat result = new();
        Cv2.MatchTemplate(liveImageMat, fixedImage, result, TemplateMatchModes.CCorrNormed);

        // 获取匹配结果的最大值
        Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out _);

        // 设置阈值，筛选出匹配程度高于阈值的区域
        //double threshold = 0.8;
        //Mat thresholdResult = new ();
        //Cv2.Threshold(result, thresholdResult, threshold, 1.0, ThresholdTypes.Binary);

        //// 计算概率
        //double probability = Cv2.CountNonZero(thresholdResult) / (double)result.Total();

        //// 打印匹配结果
        //Console.WriteLine("最佳匹配相似度: " + maxVal);
        //return probability;
        return maxVal;
    }

    private static double CalculateSimilarityByHist(Mat fixedImage, Bitmap liveImage)
    {
        Mat liveImageMat = liveImage.ToMat();

        // 计算图像1的直方图
        Mat hist1 = new();
        Cv2.CalcHist(new[] { fixedImage }, new int[] { 0 }, null, hist1, 1, new[] { 256 }, new Rangef[] { new Rangef(0, 256) });

        // 计算图像2的直方图
        Mat hist2 = new();
        Cv2.CalcHist(new[] { liveImageMat }, new int[] { 0 }, null, hist2, 1, new[] { 256 }, new Rangef[] { new Rangef(0, 256) });

        // 比较两张图像的直方图   Hellinger / Bhattacharyya
        double similarity = Cv2.CompareHist(hist1, hist2, HistCompMethods.Bhattacharyya);

        return similarity;
    }
}
