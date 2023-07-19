using System.Windows.Controls;

using CV_ViewTool.ViewModels;

namespace CV_ViewTool.Views;

public partial class BlankPage : Page
{
    public BlankPage(BlankViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
