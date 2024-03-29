﻿using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using HandyControl.Controls;
using PCReHack.Helper;
using PCReHack.Models;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace PCReHack.ViewModels
{
    public class RectViewModel : Screen, IHandle<string>
    {
        public string Title { get; set; } = "检测框";
        public double TitleHeight { get; set; } = 15;
        public double Padding { get; set; } = 10;
        public double Top { get; set; }
        public double Left { get; set; }
        public double MinHeight { get; set; } = 100;
        public double MinWidth { get; set; } = 100;
        public double Height { get; set; } = 100;
        public double Width { get; set; } = 100;
        public int NextScreenTime { get; set; } = 100;
        public ObservableCollection<TempImage> TempImageList { get; set; } = new();
        public bool IsFound { get; set; } = false;
        public double Threshold { get; set; } = 0.4;
        public int ClickInterval { get; set; } = 200;

        public ObservableCollection<MatchTemplateResult> MatchTemplateResults { get; set; } = new();

        private List<MatchTemplateResult> ScreenshutResults { get; set; } = new();
        private IEventAggregator EventAggregator { get; set; }

        public RectViewModel(IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;
            EventAggregator.Subscribe(this, nameof(RectViewModel));
        }

        public void Handle(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                switch (message.ToUpper())
                {
                    case "CLOSE":
                        {
                            RequestClose();
                            break;
                        }
                }
            }
        }

        public void SetTemplateImage(List<TempImage> temps)
        {
            TempImageList.Clear();
            TempImageList = new(temps);
            AdjustHW();
        }

        public void AdjustHW()
        {
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

        public bool LastFound { get; set; }

        public async Task<bool> ScreenshutAsync()
        {
            var isFound = false;
            try
            {
                ScreenshutResults.Clear();

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
                    IsFound = maxValues[0] > Threshold;

                    //Debug.WriteLine($"{Title}:\t{temp.Name}:\t{maxValues[0]}");

                    MatchTemplateResult match = new()
                    {
                        Title = Title,
                        Name = temp.Name,
                        Result = maxValues[0],
                        IsFound = IsFound,
                        ClickTimes = temp.Name.Contains("R_") ? 3 : temp.Name.Contains("B_") ? 2 : 1
                    };
                    //EventAggregator.Publish(match, nameof(RootViewModel));
                    _ = Execute.OnUIThreadAsync(() =>
                    {
                        MatchTemplateResults.Add(match);
                        if (MatchTemplateResults.Count > 10)
                        {
                            MatchTemplateResults.RemoveAt(0);
                        }
                    });

                    if (IsFound && !LastFound)
                    {
                        ScreenshutResults.Add(match);
                    }
                }

                if (ScreenshutResults.Count > 0 && !Title.Contains("中间"))
                {
                    var match = ScreenshutResults.Where(m => m.IsFound && m.Result > Threshold).MaxBy(m => m.Result);
                    for (int i = 0; i < match.ClickTimes; i++)
                    {
                        MouseOperations.SetCursorPosition(x: (int)(Left + Width / 2), y: (int)(Top + Height - 5));
                        MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftDown);
                        MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftUp);
                        //if (i < match.ClickTimes - 1) 
                        await Task.Delay(ClickInterval).ConfigureAwait(false);
                    }

                    await Task.Delay(NextScreenTime).ConfigureAwait(false);

                    isFound = true;
                }
                else if (ScreenshutResults.Count > 0 && Title.Contains("中间"))
                {
                    var time = DateTime.Now.AddMilliseconds(5000);
                    while (time >= DateTime.Now)
                    {
                        MouseOperations.SetCursorPosition(x: (int)(Left + Width / 2), y: (int)(Top + Height - 5));
                        MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftDown);
                        MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftUp);
                    }

                    await Task.Delay(1200).ConfigureAwait(false);

                    isFound = true;
                }

                LastFound = isFound;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            await Task.Delay(NextScreenTime).ConfigureAwait(false);
            return isFound;
        }
    }
}