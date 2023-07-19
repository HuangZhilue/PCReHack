using System.Windows;

namespace CV_ViewTool.Contracts.Services;

public interface IWindowManagerService
{
    Window MainWindow { get; }

    void OpenInNewWindow(string id, string pageKey, object parameter = null, string title = "CV_ViewTool", string shellName = "CustomMetroWindow");

    bool? OpenInDialog(string pageKey, object parameter = null);

    Window GetWindow(string pageKey);

    Window GetWindow(string pageKey, string id);

    /// <summary>
    /// 通知到窗口（需要窗口的ViewModel实现了INavigationAware接口，否则无法接收传递的数据）
    /// </summary>
    /// <param name="id">窗口id</param>
    /// <param name="pageKey">窗口的ViewModel类名</param>
    /// <param name="parameter">传递的数据</param>
    /// <param name="activate">是否激活窗口</param>
    void NotifyToWindow(string id, string pageKey, object parameter = null, bool activate = false);
}
