using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using HandyControl.Controls;
using PCReHack.Helper;
using PCReHack.Models;
using Stylet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace PCReHack.ViewModels
{
    public class RectRightViewModel : Screen, IHandle<string>
    {
        public string Title { get; set; } = "PCR 右侧检测框";
        public double TitleHeight { get; set; } = 15;
        public double Padding { get; set; } = 5;
        public double Top { get; set; }
        public double Left { get; set; }
        public double MinHeight { get; set; } = 100;
        public double MinWidth { get; set; } = 100;
        public double Height { get; set; } = 100;
        public double Width { get; set; } = 100;
        public bool IsStart { get; set; }
        public int NextScreenTime { get; set; } = 100;
        public List<TempImage> TempImageList { get; set; } = new();
        //public BitmapSource Image { get; set; } = Resource.HeadRight.BitmapToBitmapSource();
        //public Bitmap ImageBitmap { get; set; } = Resource.HeadRight;
        public bool IsFound { get; set; } = false;

        public RectRightViewModel(IEventAggregator eventAggregator)
        {
            eventAggregator.Subscribe(this);
            //Height = ImageBitmap.Height + TitleHeight;
            //Width = ImageBitmap.Width;
            //MinHeight = Height;
            //MinWidth = Width;
        }

        public void Handle(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                switch (message.ToUpper())
                {
                    case "STARTSCREENSHUT":
                        {
                            IsStart = true;
                            Task.Run(async () =>
                            {
                                await Task.Delay(1000).ConfigureAwait(false);
                                Screenshut();
                            });
                            break;
                        }
                    case "STOPSCREENSHUT":
                        {
                            IsStart = false;
                            break;
                        }
                    case "CLOSE":
                        {
                            IsStart = false;
                            RequestClose();
                            break;
                        }
                }
            }
        }

        public void SetTemplateImage(List<TempImage> temps)
        {
            TempImageList = temps;
            int maxWidth = 100, maxHeight = 100;
            if (TempImageList.Count > 0)
            {
                maxWidth = TempImageList.Max(t => t.Bitmap.Width);
                maxHeight = TempImageList.Max(t => t.Bitmap.Height);
            }

            Height = maxHeight + TitleHeight + Padding;
            Width = maxWidth + Padding;
            MinHeight = Height;
            MinWidth = Width;
        }

        public void Screenshut()
        {
            Task.Run(async () =>
            {
                while (IsStart)
                {
                    try
                    {
                        Rectangle bounds = new((int)Left, (int)(Top + TitleHeight), (int)Width, (int)(Height - TitleHeight));
                        using Bitmap bitmap = new(bounds.Width, bounds.Height);
                        using (Graphics g = Graphics.FromImage(bitmap))
                        {
                            g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
                        }

                        Image<Bgr, byte> origin = bitmap.ToImage<Bgr, byte>();

                        foreach (TempImage temp in TempImageList)
                        {
                            using Image<Gray, float> result = origin.MatchTemplate(temp.Template, TemplateMatchingType.CcoeffNormed);
                            result.MinMax(out _, out double[] maxValues, out _, out _);

                            // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.

                            Debug.WriteLine($"Right:\t{maxValues[0]}");
                            if (maxValues[0] > 0.40)
                            {
                                IsFound = true;
                            }
                            else
                            {
                                IsFound = false;
                            }
                        }

                        #region 保存图片
                        //string outputFileName = System.AppDomain.CurrentDomain.BaseDirectory + "//testRight.jpg";
                        //using (MemoryStream memory = new MemoryStream())
                        //{
                        //    using (FileStream fs = new FileStream(outputFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                        //    {
                        //        bitmap.Save(memory, ImageFormat.Jpeg);
                        //        byte[] bytes = memory.ToArray();
                        //        fs.Write(bytes, 0, bytes.Length);
                        //    }
                        //}
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        IsStart = false;
                        break;
                    }
                    finally
                    {
                        await Task.Delay(NextScreenTime).ConfigureAwait(false);
                    }
                }
            });
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
                Execute.OnUIThreadAsync(() =>
                {
                    var bitmap = new Bitmap(filename);
                    var b = bitmap.BitmapToBitmapSource();
                    if (b is not null)
                    {
                        //ImageBitmap = bitmap;
                        //Image = b;
                        //Height = ImageBitmap.Height + TitleHeight;
                        //Width = ImageBitmap.Width;
                        //MinHeight = Height;
                        //MinWidth = Width;
                    }
                });
            }
        }
    }
}