using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CV_ViewTool.Contracts.Services;
using CV_ViewTool.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace CV_ViewTool.ViewModels;

public class MainViewModel : ObservableObject
{
    public ObservableCollection<CVProfile> CVProfiles { get; set; } = new();
    private IWindowManagerService WindowManagerService { get; }
    private IToastNotificationsService ToastNotificationsService { get; }

    public MainViewModel(IServiceProvider service)
    {
        WindowManagerService = service.GetRequiredService<IWindowManagerService>();
        ToastNotificationsService = service.GetRequiredService<IToastNotificationsService>();

        RefreshItem();
    }

    private ICommand _addItemCommand;
    private ICommand _refreshItemCommand;
    private ICommand _deleteItemCommand;
    private ICommand _editItemCommand;
    private ICommand _openMonitorWindowCommand;
    public ICommand AddItemCommand => _addItemCommand ??= new RelayCommand(AddItem);
    public ICommand RefreshItemCommand => _refreshItemCommand ??= new RelayCommand(RefreshItem);
    public ICommand EditItemCommand => _editItemCommand ??= new RelayCommand<string>(EditItem);
    public ICommand DeleteItemCommand => _deleteItemCommand ??= new RelayCommand<string>(DeleteItem);
    public ICommand OpenMonitorWindowCommand => _openMonitorWindowCommand ??= new RelayCommand<string>(OpenMonitorWindow);

    private void RefreshItem()
    {
        string profileList = App.Current.Properties["ProfileList"]?.ToString() ?? "";
        List<CVProfile> profiles = JsonConvert.DeserializeObject<List<CVProfile>>(profileList) ?? new();
        CVProfiles.Clear();
        profiles.ForEach(profile => CVProfiles.Add(profile));
    }

    private void AddItem()
    {
        //WindowManagerService.NotifyToWindow("MonitorWindow_RED", typeof(MonitorViewModel).FullName, "BLUE", true);
        //return;
        bool? open = WindowManagerService.OpenInDialog(typeof(InRangeViewModel).FullName);
        if (open == true) RefreshItem();
    }

    private void EditItem(string profileName)
    {
        bool? open = WindowManagerService.OpenInDialog(typeof(InRangeViewModel).FullName, profileName);
        if (open == true) RefreshItem();
    }

    private void DeleteItem(string profileName)
    {
        if (string.IsNullOrWhiteSpace(profileName))
        {
            ToastNotificationsService.ShowToastNotification("Warning", "Empty Profile Name!");
            return;
        }

        var r = MessageBox.Show($"是否删除 ({profileName})？", "提示", MessageBoxButton.YesNo);
        if (r != MessageBoxResult.Yes) return;
        string profileList = App.Current.Properties["ProfileList"]?.ToString() ?? "";
        List<CVProfile> profiles = JsonConvert.DeserializeObject<List<CVProfile>>(profileList) ?? new();

        profiles.RemoveAll(p => string.IsNullOrWhiteSpace(p.ProfileName) || p.ProfileName.ToUpper() == profileName.ToUpper());
        App.Current.Properties["ProfileList"] = JsonConvert.SerializeObject(profiles);
        ToastNotificationsService.ShowToastNotification("Success", "Deleted!");
        RefreshItem();
    }

    private void OpenMonitorWindow(string profileName)
    {
        string name = "MonitorWindow_"+profileName??"";
        WindowManagerService.OpenInNewWindow(name, typeof(MonitorViewModel).FullName, parameter: profileName, title: name, shellName: "NoTitleMetroWindow");
    }
}
