using HandyControl.Controls;
using Stylet;
using StyletIoC;
using System;
using System.Linq;

namespace DeemoHack.ViewModels;

public class TestViewModel :  Conductor<Screen>.Collection.AllActive
{
    public string Title { get; set; } = "TestViewModel";
    public double Top { get; set; } = 200;
    public double Left { get; set; } = 200;
    public int SelectedTabIndex { get; set; }

    private IWindowManager WindowManager { get; }
    private IContainer Container { get; }

    public TestViewModel(IWindowManager windowManager, IContainer container)
    {
        WindowManager = windowManager;
        Container = container;
    }

    public void Menu2Tab(string viewModelString)
    {
        try
        {
            var item = Items.ToList().FindIndex(i => string.Equals(i.DisplayName, viewModelString, StringComparison.OrdinalIgnoreCase));
            if (item > -1)
            {
                SelectedTabIndex = item;
            }
            else
            {
                Type type = Type.GetType(
                    nameof(DeemoHack) +
                    $".{nameof(ViewModels)}" +
                    $".{viewModelString}" +
                    $", {nameof(DeemoHack)}");
                if (Container.Get(type) is Screen vm)
                {
                    vm.DisplayName = viewModelString;
                    ActivateItem(vm);
                    SelectedTabIndex = Items.Count - 1;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "错误");
        }
    }

    protected override void OnClose()
    {
        base.OnClose();
    }
}