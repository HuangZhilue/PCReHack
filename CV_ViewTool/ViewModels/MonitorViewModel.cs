using ColorPicker.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CV_ViewTool.Contracts.Services;
using CV_ViewTool.Contracts.ViewModels;
using CV_ViewTool.Helpers;
using CV_ViewTool.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OpenCvSharp;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CV_ViewTool.ViewModels;

public class PercentageRecord
{
    public DateTime DateTime { get; set; }
    public double Percentage { get; set; }
}

public class MonitorViewModel : ObservableObject, INavigationAware
{
    private bool _enableImage = true;
    private string _profileName = string.Empty;
    private bool _useInRange;
    private BitmapImage _originalImage;
    private BitmapImage _processedImage;
    private int _threshold = 125;
    private int _maxBinary = 255;
    private int _fps = 0;
    private ColorState _colorState = new();
    private ColorState _colorState2 = new();
    private ICommand _showImageCommand;
    private ICommand _setCameraCommand;
    private ICommand _initCameraListCommand;

    public bool EnableImage { get => _enableImage; set => SetProperty(ref _enableImage, value); }
    public string ProfileName { get => _profileName; set => SetProperty(ref _profileName, value); }
    public bool UseInRange { get => _useInRange; set => SetProperty(ref _useInRange, value); }
    public BitmapImage OriginalImage { get => _originalImage; set => SetProperty(ref _originalImage, value); }
    public BitmapImage ProcessedImage { get => _processedImage; set => SetProperty(ref _processedImage, value); }
    public int Threshold { get => _threshold; set => SetProperty(ref _threshold, value); }
    public int MaxBinary { get => _maxBinary; set => SetProperty(ref _maxBinary, value); }
    public int FPS { get => _fps; set => SetProperty(ref _fps, value); }
    public ColorState ColorState { get => _colorState; set => SetProperty(ref _colorState, value); }
    public ColorState ColorState2 { get => _colorState2; set => SetProperty(ref _colorState2, value); }
    public ObservableCollection<string> CameraNameList { get; } = new();
    public ObservableCollection<PercentageRecord> ProbabilityList { get; } = new();

    public ICommand ShowImageCommand => _showImageCommand??=new RelayCommand(() => { EnableImage = !EnableImage; });
    public ICommand SetCameraCommand => _setCameraCommand??=new RelayCommand<int>(SetCamera);
    public ICommand InitCameraListCommand => _initCameraListCommand ??=new RelayCommand(InitCameraList);

    private Queue<Action> Queue { get; } = new();
    private object IsImageProcessing { get; } = new();
    private IAppState AppState { get; }

    public MonitorViewModel(IServiceProvider service)
    {
        AppState = service.GetRequiredService<IAppState>();

        Task.Run(async () =>
        {
            while (true)
            {
                DateTime dateTime = DateTime.Now;
                double percentage = Random.Shared.NextDouble() * 100;
                App.Current.Dispatcher.Invoke(() =>
                {
                    while (ProbabilityList.Count >= 10)
                    {
                        ProbabilityList.RemoveAt(ProbabilityList.Count-1);
                    }
                    ProbabilityList.Insert(0, new() { DateTime =  dateTime, Percentage = percentage });
                });

                await Task.Delay(1000).ConfigureAwait(false);
            }
        });
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
    }

    public void OnNavigatedFrom()
    {
        //throw new NotImplementedException();
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
                using Mat image = OpenCvSharp.Extensions.BitmapConverter.ToMat(bitmap);
                if (!UseInRange)
                {
                    Cv2.CvtColor(image, image, ColorConversionCodes.BGR2GRAY);
                    Cv2.Threshold(image, image, Threshold, MaxBinary, ThresholdTypes.Binary);
                    bitmap2 = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(image);
                }
                else
                {
                    Cv2.CvtColor(image, image, ColorConversionCodes.BGR2HSV);
                    Cv2.InRange(
                            image,
                            new Scalar(ColorState.HSV_H / 2, ColorState.HSV_S * 255, ColorState.HSV_V * 255),
                            new Scalar(ColorState2.HSV_H / 2, ColorState2.HSV_S * 255, ColorState2.HSV_V * 255),
                            image);
                    bitmap2 = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(image);
                }

                if (bitmap2 is not null)
                {
                    //App.Current.Dispatcher.Invoke(() =>
                    //{
                    ProcessedImage = bitmap2.BitmapToBitmapImage();
                    //});
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
}
