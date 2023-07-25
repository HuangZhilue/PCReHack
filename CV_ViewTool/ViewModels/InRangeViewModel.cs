using ColorPicker.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CV_ViewTool.Contracts.Services;
using CV_ViewTool.Contracts.ViewModels;
using CV_ViewTool.Helpers;
using CV_ViewTool.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Newtonsoft.Json;
using OpenCvSharp;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using BitmapSource = System.Windows.Media.Imaging.BitmapSource;

namespace CV_ViewTool.ViewModels;

public class InRangeViewModel : ObservableObject, INavigationAware
{
    private string _title = "阈值测试";
    private string _profileName = string.Empty;
    private bool _useInRange;
    private BitmapImage _originalImage;
    private BitmapImage _processedImage;
    private Bitmap _bitmap1;
    private Bitmap _bitmap2;
    private int _threshold = 125;
    private int _maxBinary = 255;
    private ColorState _colorState = new();
    private ColorState _colorState2 = new();
    //private int width = 800;
    //private int height = 450;
    //private int maxWidth = 800;
    //private int maxHeight = 450;
    //private int minWidth = 100;
    //private int minHeight = 100;
    //private bool isMinSize = false;
    private ICommand _getImageCommand;
    private ICommand _imageProcessingCommand;
    private ICommand _saveCommand;
    private ICommand _saveSampleImageCommand;

    public string Title { get => _title; set => SetProperty(ref _title, value); }
    public string ProfileName { get => _profileName; set => SetProperty(ref _profileName, value); }
    public bool UseInRange { get => _useInRange; set => SetProperty(ref _useInRange, value); }
    public Bitmap Bitmap1 { get => _bitmap1; set => SetProperty(ref _bitmap1, value); }
    public Bitmap Bitmap2 { get => _bitmap2; set => SetProperty(ref _bitmap2, value); }
    public BitmapImage OriginalImage { get => _originalImage; set => SetProperty(ref _originalImage, value); }
    public BitmapImage ProcessedImage { get => _processedImage; set => SetProperty(ref _processedImage, value); }
    public int Threshold { get => _threshold; set => SetProperty(ref _threshold, value); }
    public int MaxBinary { get => _maxBinary; set => SetProperty(ref _maxBinary, value); }
    public ColorState ColorState { get => _colorState; set => SetProperty(ref _colorState, value); }
    public ColorState ColorState2 { get => _colorState2; set => SetProperty(ref _colorState2, value); }
    //public int Width { get => width; set => SetProperty(ref width, value); }
    //public int Height { get => height; set => SetProperty(ref height, value); }
    //public int MaxWidth { get => maxWidth; set => SetProperty(ref maxWidth, value); }
    //public int MaxHeight { get => maxHeight; set => SetProperty(ref maxHeight, value); }
    //public int MinWidth { get => minWidth; set => SetProperty(ref minWidth, value); }
    //public int MinHeight { get => minHeight; set => SetProperty(ref minHeight, value); }
    //public bool IsMinSize { get => isMinSize; set => SetProperty(ref isMinSize, value); }

    public ICommand GetImageCommand => _getImageCommand??=new RelayCommand(GetImage);
    public ICommand ImageProcessingCommand => _imageProcessingCommand??=new RelayCommand(ImageProcessing);
    public ICommand SaveCommand => _saveCommand??=new RelayCommand(Save);
    public ICommand SaveSampleImageCommand => _saveSampleImageCommand??=new RelayCommand(SaveSampleImage);

    private Queue<Action> Queue { get; } = new();
    private object IsImageProcessing { get; } = new();

    private IToastNotificationsService ToastNotificationsService { get; }

    public InRangeViewModel(IServiceProvider service)
    {
        ToastNotificationsService = service.GetRequiredService<IToastNotificationsService>();
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
    }

    public void OnNavigatedFrom()
    {
        //throw new NotImplementedException();
    }

    private void GetImage()
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

        var bitmap = new Bitmap(filename);
        Bitmap1 = bitmap;
        OriginalImage = bitmap.BitmapToBitmapImage();
    }

    private void ImageProcessing()
    {
        if (Bitmap1 is null) return;

        Queue.Enqueue((() =>
        {
            using Mat image = OpenCvSharp.Extensions.BitmapConverter.ToMat(Bitmap1);
            if (!UseInRange)
            {
                Cv2.CvtColor(image, image, ColorConversionCodes.BGR2GRAY);
                Cv2.Threshold(image, image, Threshold, MaxBinary, ThresholdTypes.Binary);
                Bitmap2 = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(image);
            }
            else
            {
                Cv2.CvtColor(image, image, ColorConversionCodes.BGR2HSV);
                Cv2.InRange(
                    image,
                    new Scalar(ColorState.HSV_H / 2, ColorState.HSV_S * 255, ColorState.HSV_V * 255),
                    new Scalar(ColorState2.HSV_H / 2, ColorState2.HSV_S * 255, ColorState2.HSV_V * 255),
                    image);
                Bitmap2 = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(image);
            }

            if (Bitmap2 is not null)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    ProcessedImage = Bitmap2.BitmapToBitmapImage();
                });
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

    private void Save()
    {
        if (string.IsNullOrWhiteSpace(ProfileName))
        {
            ToastNotificationsService.ShowToastNotification("Warning", "Empty Profile Name!");
            return;
        }

        string profileList = App.Current.Properties["ProfileList"]?.ToString() ?? "";
        List<CVProfile> profiles = JsonConvert.DeserializeObject<List<CVProfile>>(profileList) ?? new();
        if (profiles.FirstOrDefault(p => p.ProfileName.ToUpper() == ProfileName.ToUpper()) is not CVProfile profile)
            profile = new()
            {
                ProfileName = ProfileName
            };

        profile.MaxBinary = MaxBinary;
        profile.UseInRange = UseInRange;
        profile.Threshold = Threshold;
        profile.ColorState = ColorState;
        profile.ColorState2 = ColorState2;
        profiles.RemoveAll(p => string.IsNullOrWhiteSpace(p.ProfileName) || p.ProfileName.ToUpper() == ProfileName.ToUpper());
        profiles.Add(profile);
        App.Current.Properties["ProfileList"] = JsonConvert.SerializeObject(profiles);

        ToastNotificationsService.ShowToastNotification("Success", "Saved!");
    }

    private void SaveSampleImage()
    {
        // 创建一个SaveFileDialog实例
        SaveFileDialog saveFileDialog = new()
        {
            AddExtension = true,
            FileName = "CV_ViewTool_Sample_" + DateTime.Now.ToString("HHmmss"),
            Filter = "PNG 图片文件 (*.png)|*.png"
        };

        // 打开文件保存对话框
        bool? result = saveFileDialog.ShowDialog();

        // 如果用户点击了保存按钮
        if (result == true)
        {
            // 获取用户选择的文件路径
            Bitmap2.Save(saveFileDialog.FileName, ImageFormat.Png);
        }
    }
}
