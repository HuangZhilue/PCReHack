using ColorPicker.Models;
using CV_Hack.Helper;
using OpenCvSharp;
using Stylet;
using StyletIoC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using BitmapSource = System.Windows.Media.Imaging.BitmapSource;

namespace CV_Hack.ViewModels.Test.OpenCV;

public class InRangeViewModel : Screen
{
    public string Title { get; set; } = "阈值测试";
    public bool UseInRange { get; set; }
    public int SelectedTabIndex { get; set; }
    public BitmapSource BitmapSource { get; set; }
    public Bitmap Bitmap1 { get; set; }
    public BitmapSource BitmapSource2 { get; set; }
    public Bitmap Bitmap2 { get; set; }

    public int Threshold { get; set; } = 125;
    public int MaxBinary { get; set; } = 255;

    public ColorState ColorState { get; set; } = new();
    public ColorState ColorState2 { get; set; } = new();
    //#region RGB
    //public double ColorR1 { get; set; } = 125;
    //public double ColorG1 { get; set; } = 125;
    //public double ColorB1 { get; set; } = 125;
    //public double ColorR2 { get; set; } = 125;
    //public double ColorG2 { get; set; } = 125;
    //public double ColorB2 { get; set; } = 125;
    //#endregion

    private IWindowManager WindowManager { get; }
    private IContainer Container { get; }
    private Queue<Action> Queue { get; } = new();
    private object IsImageProcessing { get; } = new();

    public InRangeViewModel(IWindowManager windowManager, IContainer container)
    {
        WindowManager = windowManager;
        Container = container;
    }

    public void GetImage()
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
        if (result == true)
        {
            // Open document 
            string filename = dlg.FileName;

            var bitmap = new Bitmap(filename);
            Bitmap1 = bitmap;
            BitmapSource = bitmap.BitmapToBitmapSource();
        }
    }

    public void ImageProcessing()
    {
        //Debug.WriteLine($"RGBTb:{ColorR1}\t{ColorG1}\t{ColorB1}\t{Threshold}\t{MaxBinary}");
        if (BitmapSource is not null && Bitmap1 is not null)
        {
            Queue.Enqueue((() =>
            {
                using Mat image = OpenCvSharp.Extensions.BitmapConverter.ToMat(Bitmap1);
                if (!UseInRange)
                {
                    //using Mat resultImage = new();
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
                    Execute.OnUIThreadAsync(() =>
                    {
                        BitmapSource2 = Bitmap2.BitmapToBitmapSource();
                    });
                }
            }));
            RunImageProcessingQueue();
        }
    }

    public void RunImageProcessingQueue()
    {
        lock (IsImageProcessing)
        {
            while (Queue.Count > 0)
            {
                //Debug.WriteLine("IsImageProcessing...");
                Queue.Dequeue().Invoke();
            }
        }
    }

    protected override void OnClose()
    {
        base.OnClose();
    }
}