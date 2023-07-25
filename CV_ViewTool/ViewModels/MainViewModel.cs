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
    private bool _isOpenNewScreenCaptureWindow = true;
    private bool _isStart = false;
    public bool IsOpenNewScreenCaptureWindow
    {
        get => _isOpenNewScreenCaptureWindow;
        set { SetProperty(ref _isOpenNewScreenCaptureWindow, value); }
    }

    public bool IsStart
    {
        get => _isStart;
        set { SetProperty(ref _isStart, value); }
    }

    public ObservableCollection<CVProfile> CVProfiles { get; set; } = new();
    private IWindowManagerService WindowManagerService { get; }
    private IToastNotificationsService ToastNotificationsService { get; }
    private IAppState AppState { get; }

    public MainViewModel(IServiceProvider service)
    {
        WindowManagerService = service.GetRequiredService<IWindowManagerService>();
        ToastNotificationsService = service.GetRequiredService<IToastNotificationsService>();
        AppState = service.GetRequiredService<IAppState>();

        RefreshItem();
    }

    private ICommand _addItemCommand;
    private ICommand _openNewScreenCaptureWindowCommand;
    private ICommand _refreshItemCommand;
    private ICommand _deleteItemCommand;
    private ICommand _editItemCommand;
    private ICommand _openMonitorWindowCommand;
    private ICommand _startCommand;
    public ICommand AddItemCommand => _addItemCommand ??= new RelayCommand(AddItem);
    public ICommand OpenNewScreenCaptureWindowCommand => _openNewScreenCaptureWindowCommand ??= new AsyncRelayCommand(OpenNewScreenCaptureWindow, () => IsOpenNewScreenCaptureWindow);
    public ICommand RefreshItemCommand => _refreshItemCommand ??= new RelayCommand(RefreshItem);
    public ICommand EditItemCommand => _editItemCommand ??= new RelayCommand<string>(EditItem);
    public ICommand DeleteItemCommand => _deleteItemCommand ??= new RelayCommand<string>(DeleteItem);
    public ICommand OpenMonitorWindowCommand => _openMonitorWindowCommand ??= new RelayCommand<string>(OpenMonitorWindow);
    public ICommand StartCommand => _startCommand ??= new RelayCommand(Start);

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

    private async Task OpenNewScreenCaptureWindow()
    {
        IsOpenNewScreenCaptureWindow = false;

        DateTime dateTime = DateTime.Now;
        string title = "Screen Capture " + dateTime.ToString("HH:mm:ss:FFFFFFF");
        string id = (typeof(ScreenCaptureViewModel).FullName + dateTime.Ticks + "." + Random.Shared.Next(0, 9)).Replace(".", "_");
        WindowManagerService.OpenInNewWindow(
            id,
            typeof(ScreenCaptureViewModel).FullName,
            parameter: new Dictionary<string, object>() { { "Id", id } },
            title: title,
            shellName: "LowTitleHeightTopMostTransparencyMetroWindow");
        AppState.CameraState.TryAdd(title, id);
        AppState.CameraMonitorState.TryAdd(title, new());
        await Task.Delay(200).ConfigureAwait(false);

        IsOpenNewScreenCaptureWindow = true;
    }



    private void EditItem(string profileName)
    {
        bool? open = WindowManagerService.OpenInDialog(
            typeof(InRangeViewModel).FullName,
            parameter: new Dictionary<string, object>() { { "ProfileName", profileName } });
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
        WindowManagerService.OpenInNewWindow(
            name,
            typeof(MonitorViewModel).FullName,
            parameter: new Dictionary<string, object>() { { "ProfileName", profileName } },//profileName, 
            title: name,
            shellName: "TransparencyMetroWindow");
    }

    private void Start()
    {
        List<string> idList = AppState.CameraState.Values.ToList();
        if (idList.Count <= 0)
        {
            IsStart = false;
            return;
        }

        IsStart = !IsStart;

        foreach (var id in idList)
        {
            WindowManagerService.NotifyToWindow(
                id,
                typeof(ScreenCaptureViewModel).FullName,
                parameter: new Dictionary<string, object>() { { "IsStart", IsStart } });
        }
    }
}
