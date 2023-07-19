using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

using CV_ViewTool.Contracts.Services;
using CV_ViewTool.Contracts.ViewModels;
using CV_ViewTool.Contracts.Views;

using MahApps.Metro.Controls;

namespace CV_ViewTool.Services;

public class WindowManagerService : IWindowManagerService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPageService _pageService;

    public Window MainWindow
        => Application.Current.MainWindow;

    public WindowManagerService(IServiceProvider serviceProvider, IPageService pageService)
    {
        _serviceProvider = serviceProvider;
        _pageService = pageService;
    }

    public void OpenInNewWindow(string id, string key, object parameter = null, string title = "CV_ViewTool", string shellName = "CustomMetroWindow")
    {
        var window = GetWindow(key, id);
        if (window != null && window.Name == id)
        {
            window.Activate();
        }
        else
        {
            window = new MetroWindow()
            {
                Title = title,
                Style = Application.Current.FindResource(shellName) as Style,
                Name = id,
            };
            var frame = new Frame()
            {
                Focusable = false,
                NavigationUIVisibility = NavigationUIVisibility.Hidden
            };

            window.Content = frame;
            var page = _pageService.GetPage(key);
            window.Closed += OnWindowClosed;
            window.Show();
            frame.Navigated += OnNavigated;
            var navigated = frame.Navigate(page, parameter);
        }
    }

    public bool? OpenInDialog(string key, object parameter = null)
    {
        var shellWindow = _serviceProvider.GetService(typeof(IShellDialogWindow)) as Window;
        var frame = ((IShellDialogWindow)shellWindow).GetDialogFrame();
        frame.Navigated += OnNavigated;
        shellWindow.Closed += OnWindowClosed;
        var page = _pageService.GetPage(key);
        var navigated = frame.Navigate(page, parameter);
        return shellWindow.ShowDialog();
    }

    public Window GetWindow(string key)
    {
        foreach (Window window in Application.Current.Windows)
        {
            var dataContext = window.GetDataContext();
            if (dataContext?.GetType().FullName == key)
            {
                return window;
            }
        }

        return null;
    }

    public Window GetWindow(string key, string id)
    {
        foreach (Window window in Application.Current.Windows)
        {
            var dataContext = window.GetDataContext();
            if (dataContext?.GetType().FullName == key && window.Name == id)
            {
                return window;
            }
        }

        return null;
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        if (sender is Frame frame)
        {
            var dataContext = frame.GetDataContext();
            if (dataContext is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedTo(e.ExtraData);
            }
        }
    }

    private void OnWindowClosed(object sender, EventArgs e)
    {
        if (sender is Window window)
        {
            if (window.Content is Frame frame)
            {
                frame.Navigated -= OnNavigated;
            }

            window.Closed -= OnWindowClosed;
        }
    }

    public void NotifyToWindow(string id, string key, object parameter = null, bool activate = false)
    {
        var window = GetWindow(key, id);
        if (window != null && window.Name == id)
        {
            object dataContext = window.GetDataContext();
            if (dataContext is null) return;
            (dataContext as INavigationAware)?.OnNavigatedTo(parameter);

            if (activate) window.Activate();
        }
    }
}
