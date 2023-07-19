using System.Windows.Controls;

using CV_ViewTool.ViewModels;

namespace CV_ViewTool.Views;

public partial class InRangePage : Page
{
    public InRangePage(InRangeViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
