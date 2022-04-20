using Emgu.CV;
using Emgu.CV.Structure;
using PCReHack.Helper;
using PCReHack.Models;
using PCReHack.Properties;
using Stylet;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PCReHack.ViewModels
{
    public class RootViewModel : Screen, IHandle<MatchTemplateResult>
    {
        public string Title { get; set; } = "PCReHack";
        public double Top { get; set; } = 200;
        public double Left { get; set; } = 200;
        public double Threshold { get; set; } = 0.4;
        public int ClickInterval { get; set; } = 200;
        public int NextScreenTime { get; set; } = 200;
        public bool IsStart { get; set; }

        public RectViewModel LeftRectViewModel { get; set; }
        public RectViewModel RightRectViewModel { get; set; }
        public RectViewModel CenterRectViewModel { get; set; }

        private IEventAggregator EventAggregator { get; set; }
        private IWindowManager WindowManager { get; set; }
        public RootViewModel(IEventAggregator eventAggregator, IWindowManager windowManager)
        {
            EventAggregator = eventAggregator;
            EventAggregator.Subscribe(this, nameof(RootViewModel));
            WindowManager = windowManager;

            LeftRectViewModel = new(EventAggregator)
            {
                Title = "左侧检测框",
                Top = Top + 100,
                Left = Left + 100,
            };
            RightRectViewModel = new(EventAggregator)
            {
                Title = "右侧检测框",
                Top = Top + 100,
                Left = Left + 300,
            };
            CenterRectViewModel = new(EventAggregator)
            {
                Title = "中间检测框",
                Top = Top + 300,
                Left = Left + 200,
            };
        }

        public void Handle(MatchTemplateResult message)
        {
            if (message is not null)
            {
                if (message.Title == LeftRectViewModel.Title)
                {

                }
                else if (message.Title == RightRectViewModel.Title)
                {

                }
                else if (message.Title == CenterRectViewModel.Title)
                {

                }
            }
        }

        public void Start()
        {
            if (!IsStart)
            {
                IsStart = true;
                LeftRectViewModel.NextScreenTime = NextScreenTime;
                LeftRectViewModel.ClickInterval = ClickInterval;
                LeftRectViewModel.Threshold = Threshold;
                RightRectViewModel.NextScreenTime = NextScreenTime;
                RightRectViewModel.ClickInterval = ClickInterval;
                RightRectViewModel.Threshold = Threshold;
                CenterRectViewModel.NextScreenTime = NextScreenTime;
                CenterRectViewModel.ClickInterval = ClickInterval;
                CenterRectViewModel.Threshold = Threshold;
                Task.Run(async () =>
                {
                    while (IsStart)
                    {
                        if (!await LeftRectViewModel.ScreenshutAsync().ConfigureAwait(false))
                        {
                            await RightRectViewModel.ScreenshutAsync().ConfigureAwait(false);
                        }
                    }
                });
                Task.Run(async () =>
                {
                    while (IsStart)
                    {
                        await CenterRectViewModel.ScreenshutAsync().ConfigureAwait(false);
                    }
                });
            }
            else
            {
                IsStart = false;
            }
        }

        public void GetImage(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return;
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
                var b = bitmap.BitmapToBitmapSource();
                if (b is not null)
                {
                    RectViewModel viewModel = null;
                    if (title == LeftRectViewModel.Title) { viewModel = LeftRectViewModel; }
                    else if (title == RightRectViewModel.Title) { viewModel = LeftRectViewModel; }
                    else if (title == CenterRectViewModel.Title) { viewModel = LeftRectViewModel; }
                    if (viewModel is null) return;

                    viewModel.TempImageList.Add(
                        new()
                        {
                            Bitmap = bitmap,
                            Image = b,
                            Name = filename,
                            Template = bitmap.ToImage<Bgr, byte>()
                        });
                    viewModel.AdjustHW();
                }
            }
        }

        public void RemoveImageL(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var image = LeftRectViewModel.TempImageList.FirstOrDefault(i => i.Name == name);
                if (image is not null)
                {
                    LeftRectViewModel.TempImageList.Remove(image);
                    LeftRectViewModel.AdjustHW();
                }
            }
        }

        public void RemoveImageR(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var image = RightRectViewModel.TempImageList.FirstOrDefault(i => i.Name == name);
                if (image is not null)
                {
                    RightRectViewModel.TempImageList.Remove(image);
                    RightRectViewModel.AdjustHW();
                }
            }
        }

        public void RemoveImageC(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var image = CenterRectViewModel.TempImageList.FirstOrDefault(i => i.Name == name);
                if (image is not null)
                {
                    CenterRectViewModel.TempImageList.Remove(image);
                    CenterRectViewModel.AdjustHW();
                }
            }
        }

        //public void RemoveView(string title)
        //{
        //    if (!string.IsNullOrWhiteSpace(title))
        //    {
        //        var view = ViewModelList.FirstOrDefault(v => v.Title == title);
        //        if (view is not null)
        //        {
        //            ViewModelList.Remove(view);
        //        }
        //    }
        //}

        //public void GetImage(string title)
        //{
        //    if (!string.IsNullOrWhiteSpace(title))
        //    {
        //        Microsoft.Win32.OpenFileDialog dlg = new()
        //        {
        //            // Set filter for file extension and default file extension 
        //            DefaultExt = ".png",
        //            Filter = "Image Files|*.jpeg;*.png;*.jpg|JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg"
        //        };

        //        // Display OpenFileDialog by calling ShowDialog method 
        //        bool? result = dlg.ShowDialog();

        //        // Get the selected file name and display in a TextBox 
        //        if (result == true)
        //        {
        //            // Open document 
        //            string filename = dlg.FileName;

        //            var bitmap = new Bitmap(filename);
        //            var b = bitmap.BitmapToBitmapSource();
        //            if (b is not null)
        //            {
        //                var view = ViewModelList.FirstOrDefault(v => v.Title == title);
        //                if (view is not null)
        //                {
        //                    view.TempImageList.Add(
        //                        new()
        //                        {
        //                            Bitmap = bitmap,
        //                            Image = b,
        //                            Name = filename,
        //                            Template = bitmap.ToImage<Bgr, byte>()
        //                        });
        //                    view.AdjustHW();
        //                }
        //            }
        //        }
        //    }
        //}

        //public void RemoveImage(object parameter)
        //{
        //    var values = (object[])parameter;
        //    var title = values[0].ToString();
        //    var name = values[1].ToString();
        //    if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(name))
        //    {
        //        var view = ViewModelList.FirstOrDefault(v => v.Title == title);
        //        if (view is not null)
        //        {
        //            var image = view.TempImageList.FirstOrDefault(i => i.Name == name);
        //            if (image is not null)
        //            {
        //                view.TempImageList.Remove(image);
        //                view.AdjustHW();
        //            }
        //        }
        //    }
        //}

        protected override void OnViewLoaded()
        {
            var image1 = new List<TempImage>(){
                new TempImage()
                {
                    Bitmap = Resource.G_L,
                    Image = Resource.G_L.BitmapToBitmapSource(),
                    Name = nameof(Resource.G_L),
                    Template = Resource.G_L.ToImage<Bgr, byte>()
                },
                new TempImage()
                {
                    Bitmap = Resource.B_L,
                    Image = Resource.B_L.BitmapToBitmapSource(),
                    Name = nameof(Resource.B_L),
                    Template = Resource.B_L.ToImage<Bgr, byte>()
                },
                new TempImage()
                {
                    Bitmap = Resource.R_L,
                    Image = Resource.R_L.BitmapToBitmapSource(),
                    Name = nameof(Resource.R_L),
                    Template = Resource.R_L.ToImage<Bgr, byte>()
                },
            };
            var image2 = new List<TempImage>(){
                new TempImage()
                {
                    Bitmap = Resource.G_R,
                    Image = Resource.G_R.BitmapToBitmapSource(),
                    Name = nameof(Resource.G_R),
                    Template = Resource.G_R.ToImage<Bgr, byte>()
                },
                new TempImage()
                {
                    Bitmap = Resource.B_R,
                    Image = Resource.B_R.BitmapToBitmapSource(),
                    Name = nameof(Resource.B_R),
                    Template = Resource.B_R.ToImage<Bgr, byte>()
                },
                new TempImage()
                {
                    Bitmap = Resource.R_R,
                    Image = Resource.R_R.BitmapToBitmapSource(),
                    Name = nameof(Resource.R_R),
                    Template = Resource.R_R.ToImage<Bgr, byte>()
                },
            };
            var image3 = new List<TempImage>(){
                new TempImage()
                {
                    Bitmap = Resource.FEVER,
                    Image = Resource.FEVER.BitmapToBitmapSource(),
                    Name = nameof(Resource.FEVER),
                    Template = Resource.FEVER.ToImage<Bgr, byte>()
                }
            };

            LeftRectViewModel.SetTemplateImage(image1);
            RightRectViewModel.SetTemplateImage(image2);
            CenterRectViewModel.SetTemplateImage(image3);

            WindowManager.ShowWindow(LeftRectViewModel);
            WindowManager.ShowWindow(RightRectViewModel);
            WindowManager.ShowWindow(CenterRectViewModel);

            base.OnViewLoaded();
        }

        protected override void OnClose()
        {
            LeftRectViewModel.RequestClose();
            RightRectViewModel.RequestClose();
            CenterRectViewModel.RequestClose();
            base.OnClose();
        }
    }
}
