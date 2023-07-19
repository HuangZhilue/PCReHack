using System.Windows.Controls;

namespace CV_ViewTool.Contracts.Services;

public interface IPageService
{
    Type GetPageType(string key);

    Page GetPage(string key);
}
