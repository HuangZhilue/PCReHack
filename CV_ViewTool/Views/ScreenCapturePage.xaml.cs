using CV_ViewTool.ViewModels;
using System.Windows.Controls;

namespace CV_ViewTool.Views;

public partial class ScreenCapturePage : Page
{
    public ScreenCapturePage(ScreenCaptureViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
