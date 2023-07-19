using CommunityToolkit.WinUI.Notifications;

using CV_ViewTool.Contracts.Services;

using Windows.UI.Notifications;

namespace CV_ViewTool.Services;

public partial class ToastNotificationsService : IToastNotificationsService
{
    public ToastNotificationsService()
    {
    }

    public void ShowToastNotification(ToastNotification toastNotification)
    {
        ToastNotificationManagerCompat.CreateToastNotifier().Show(toastNotification);
    }
}
