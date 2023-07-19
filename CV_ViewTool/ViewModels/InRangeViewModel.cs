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
using System.Collections;
using System.Drawing;
using System.Windows.Input;
using BitmapSource = System.Windows.Media.Imaging.BitmapSource;

namespace CV_ViewTool.ViewModels;

public class InRangeViewModel : ObservableObject, INavigationAware
{
    private string title = "阈值测试";
    private string profileName = string.Empty;
    private bool useInRange;
    private BitmapSource bitmapSource;
    private Bitmap bitmap1;
    private BitmapSource bitmapSource2;
    private Bitmap bitmap2;
    private int threshold = 125;
    private int maxBinary = 255;
    private ColorState colorState = new();
    private ColorState colorState2 = new();
    //private int width = 800;
    //private int height = 450;
    //private int maxWidth = 800;
    //private int maxHeight = 450;
    //private int minWidth = 100;
    //private int minHeight = 100;
    //private bool isMinSize = false;
    private ICommand getImageCommand;
    private ICommand imageProcessingCommand;
    private ICommand saveCommand;

    public string Title { get => title; set => SetProperty(ref title, value); }
    public string ProfileName { get => profileName; set => SetProperty(ref profileName, value); }
    public bool UseInRange { get => useInRange; set => SetProperty(ref useInRange, value); }
    public BitmapSource BitmapSource { get => bitmapSource; set => SetProperty(ref bitmapSource, value); }
    public Bitmap Bitmap1 { get => bitmap1; set => SetProperty(ref bitmap1, value); }
    public BitmapSource BitmapSource2 { get => bitmapSource2; set => SetProperty(ref bitmapSource2, value); }
    public Bitmap Bitmap2 { get => bitmap2; set => SetProperty(ref bitmap2, value); }
    public int Threshold { get => threshold; set => SetProperty(ref threshold, value); }
    public int MaxBinary { get => maxBinary; set => SetProperty(ref maxBinary, value); }
    public ColorState ColorState { get => colorState; set => SetProperty(ref colorState, value); }
    public ColorState ColorState2 { get => colorState2; set => SetProperty(ref colorState2, value); }
    //public int Width { get => width; set => SetProperty(ref width, value); }
    //public int Height { get => height; set => SetProperty(ref height, value); }
    //public int MaxWidth { get => maxWidth; set => SetProperty(ref maxWidth, value); }
    //public int MaxHeight { get => maxHeight; set => SetProperty(ref maxHeight, value); }
    //public int MinWidth { get => minWidth; set => SetProperty(ref minWidth, value); }
    //public int MinHeight { get => minHeight; set => SetProperty(ref minHeight, value); }
    //public bool IsMinSize { get => isMinSize; set => SetProperty(ref isMinSize, value); }

    public ICommand GetImageCommand => getImageCommand??=new RelayCommand(GetImage);
    public ICommand ImageProcessingCommand => imageProcessingCommand??=new RelayCommand(ImageProcessing);
    public ICommand SaveCommand => saveCommand??=new RelayCommand(Save);

    private Queue<Action> Queue { get; } = new();
    private object IsImageProcessing { get; } = new();

    private IToastNotificationsService ToastNotificationsService { get; }

    public InRangeViewModel(IServiceProvider service)
    {
        ToastNotificationsService = service.GetRequiredService<IToastNotificationsService>();
    }

    public void OnNavigatedTo(object parameter)
    {
        if (parameter is not string) return;
        ProfileName = parameter.ToString();
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

    public void OnNavigatedFrom()
    {
        //throw new NotImplementedException();
    }

    private void GetImage()
    {
        Microsoft.Win32.OpenFileDialog dlg = new()
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
        BitmapSource = bitmap.BitmapToBitmapSource();
    }

    private void ImageProcessing()
    {
        if (BitmapSource is null || Bitmap1 is null) return;

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
                    BitmapSource2 = Bitmap2.BitmapToBitmapSource();
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
}
