using CommunityToolkit.Mvvm.ComponentModel;
using CV_ViewTool.Contracts.Services;
using CV_ViewTool.Contracts.ViewModels;
using MahApps.Metro.Controls;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using Point = System.Drawing.Point;

namespace CV_ViewTool.ViewModels;

public class ScreenCaptureViewModel : ObservableObject, INavigationAware
{
    private IWindowManagerService WindowManagerService { get; }
    private IAppState AppState { get; }
    private string ThisWindowId { get; set; } = "";
    private string ThisWindowName { get; set; } = "";
    private MetroWindow ThisWindow { get; set; }
    private bool IsStart { get; set; }
    private int ScreenshotCount { get; set; } = 0;
    private DateTime StartTime { get; set; }
    private List<INavigationAware> MonitorNavigationAwareList { get; set; } = new();
    private int Left { get; set; } = 0;
    private int Top { get; set; } = 0;
    private int Height { get; set; } = 0;
    private int Width { get; set; } = 0;
    private int TitleBarHeight { get; set; } = 0;

    public ScreenCaptureViewModel(IServiceProvider service)
    {
        WindowManagerService = service.GetRequiredService<IWindowManagerService>();
        AppState = service.GetRequiredService<IAppState>();
    }

    public void OnNavigatedTo(object parameter)
    {
        if (parameter is not Dictionary<string, object> dic) return;
        if (dic.ContainsKey("Id") && dic["Id"] is string id)
        {
            ThisWindow = WindowManagerService.GetWindow(typeof(ScreenCaptureViewModel).FullName, id) as MetroWindow;
            ThisWindowId = id;
            ThisWindowName = AppState.CameraState.FirstOrDefault(f => f.Value == id).Key;
            ThisWindow.Closing += ((sender, e) =>
            {
                IsStart = false;
                if (!AppState.CameraState.Values.Any(a => a == ThisWindowId)) return;
                AppState.CameraState.TryRemove(ThisWindowName, out _);
                AppState.CameraMonitorState.TryRemove(ThisWindowName, out _);
            });
            TitleBarHeight = (int)ThisWindow.TitleBarHeight;
            Width = (int)ThisWindow.Width;
            Height = (int)ThisWindow.Height;
            Top = (int)ThisWindow.Top;
            Left = (int)ThisWindow.Left;
            DependencyPropertyDescriptor.FromProperty(MetroWindow.WidthProperty, typeof(MetroWindow)).AddValueChanged(ThisWindow, (sender, e) =>
            {
                Width = (int)ThisWindow.Width;
            });
            DependencyPropertyDescriptor.FromProperty(MetroWindow.HeightProperty, typeof(MetroWindow)).AddValueChanged(ThisWindow, (sender, e) =>
            {
                Height = (int)ThisWindow.Height;
            });
            DependencyPropertyDescriptor.FromProperty(MetroWindow.TopProperty, typeof(MetroWindow)).AddValueChanged(ThisWindow, (sender, e) =>
            {
                Top = (int)ThisWindow.Top;
            });
            DependencyPropertyDescriptor.FromProperty(MetroWindow.LeftProperty, typeof(MetroWindow)).AddValueChanged(ThisWindow, (sender, e) =>
            {
                Left = (int)ThisWindow.Left;
            });
            DependencyPropertyDescriptor.FromProperty(MetroWindow.TitleBarHeightProperty, typeof(MetroWindow)).AddValueChanged(ThisWindow, (sender, e) =>
            {
                TitleBarHeight = (int)ThisWindow.TitleBarHeight;
            });
        }
        if (dic.ContainsKey("IsStart") && dic["IsStart"] is bool isStart)
        {
            IsStart = isStart;

            List<string> monitorIdList = AppState.CameraMonitorState[ThisWindowName].ToList();
            MonitorNavigationAwareList.Clear();
            foreach (string monitorId in monitorIdList)
            {
                if (WindowManagerService.GetWindow(typeof(MonitorViewModel).FullName, monitorId) is MetroWindow window)
                {
                    object dataContext = window.GetDataContext();
                    if (dataContext is null || dataContext is not INavigationAware navigationAware) continue;
                    MonitorNavigationAwareList.Add(navigationAware);
                }
            }

            Task.Run(() => ScreenShut());
        }
    }

    public void OnNavigatedFrom()
    {
        //throw new NotImplementedException();
    }

    private async void ScreenShut()
    {
        StartTime = DateTime.Now;
        ScreenshotCount = 0;

        string monitorName = typeof(MonitorViewModel).FullName;
        var dic = new Dictionary<string, object>() { { "ScreenShut", null }, { "FPS", 0 } };
        double fps = 0;

        Rectangle bounds = new(
        Left,
        Top + TitleBarHeight,
        Width,
        Height - TitleBarHeight);
        Bitmap bitmap = new(bounds.Width, bounds.Height);

        int currentWidth = Width;
        int currentHeight = Height;
        int currentTop = Top;
        int currentLeft = Left;
        int currentTitleBarHeight = TitleBarHeight;

        while (IsStart)
        {
            using Graphics g = Graphics.FromImage(bitmap);
            g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
            //App.Current.Dispatcher.Invoke(() =>
            //{
            dic["ScreenShut"] = bitmap;
            dic["FPS"] = fps;
            try
            {
                MonitorNavigationAwareList?.ForEach((navigationAware) =>
                {
                    navigationAware?.OnNavigatedTo(dic);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                // ignore
            }
            //});
            // 增加截图计数
            ScreenshotCount++;

            // 计算时间间隔
            TimeSpan elapsedTime = DateTime.Now - StartTime;

            if (elapsedTime.TotalMilliseconds > 1000)
            {
                // 计算截图频率
                fps = (ScreenshotCount / elapsedTime.TotalMilliseconds) * 1000;

                // 打印截图频率
                Debug.WriteLine($"截图频率: {fps} FPS");
                StartTime = DateTime.Now;
                ScreenshotCount=0;
            }

            // 检查 Width 和 Height 是否发生变化
            if (currentWidth != Width
                || currentHeight != Height
                || currentTitleBarHeight != TitleBarHeight
                || currentTop != Top
                || currentLeft != Left)
            {
                // 更新 bounds 的值
                bounds.X = Left;
                bounds.Y = Top + TitleBarHeight;
                bounds.Width = Width;
                bounds.Height = Height - TitleBarHeight;
                // 重新创建 bitmap 对象
                bitmap.Dispose();
                bitmap = new(bounds.Width, bounds.Height);
            }

            // 等待一定时间间隔
            await Task.Delay(TimeSpan.FromMilliseconds(0));
        }

        bitmap.Dispose();
    }
}
