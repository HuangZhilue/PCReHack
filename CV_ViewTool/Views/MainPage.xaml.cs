using System.Windows.Controls;

using CV_ViewTool.ViewModels;

namespace CV_ViewTool.Views;

public partial class MainPage : Page
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
