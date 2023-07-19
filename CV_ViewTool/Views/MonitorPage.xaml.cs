using System.Windows.Controls;

using CV_ViewTool.ViewModels;

namespace CV_ViewTool.Views;

public partial class MonitorPage : Page
{
    public MonitorPage(MonitorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
