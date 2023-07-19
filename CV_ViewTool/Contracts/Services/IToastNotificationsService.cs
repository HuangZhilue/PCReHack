using System.Windows;
using Windows.UI.Notifications;

namespace CV_ViewTool.Contracts.Services;

public interface IToastNotificationsService
{
    public abstract void ShowToastNotification(ToastNotification toastNotification);

    public abstract void ShowToastNotificationSample();

    public abstract void ShowToastNotification(string title, string content);
}
