using System.Windows.Controls;

using CV_ViewTool.ViewModels;

namespace CV_ViewTool.Views;

public partial class SettingsPage : Page
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
