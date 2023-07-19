using ColorPicker.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CV_ViewTool.Contracts.Services;
using CV_ViewTool.Contracts.ViewModels;
using CV_ViewTool.Helpers;
using CV_ViewTool.Models;
using CV_ViewTool.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OpenCvSharp;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace CV_ViewTool.ViewModels;

public class ProbabilityRecord
{
    public DateTime DateTime { get; set; }
    public double Percentage { get; set; }
}

public class MonitorViewModel : ObservableObject, INavigationAware
{
    private bool enableImage = true;
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
    private ICommand imageProcessingCommand;
    private ICommand showImageCommand;

    public bool EnableImage { get => enableImage; set => SetProperty(ref enableImage, value); }
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
    public ObservableCollection<ProbabilityRecord> ProbabilityList { get; } = new();

    public ICommand ImageProcessingCommand => imageProcessingCommand??=new RelayCommand(ImageProcessing);
    public ICommand ShowImageCommand => showImageCommand??=new RelayCommand(() => { EnableImage = !EnableImage; });

    private Queue<Action> Queue { get; } = new();
    private object IsImageProcessing { get; } = new();

    public MonitorViewModel(IServiceProvider service)
    {
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
        if (parameter is not string) return;
        if (string.IsNullOrWhiteSpace(parameter.ToString())) return;
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

    private void ImageProcessing()
    {
        if (!EnableImage || BitmapSource is null || Bitmap1 is null) return;

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
}
