﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CV_ViewTool.Contracts.Services;
using System.Windows;
using System.Windows.Input;

namespace CV_ViewTool.ViewModels;

// You can show pages in different ways (update main view, navigate, right pane, new windows or dialog)
// using the NavigationService, RightPaneService and WindowManagerService.
// Read more about MenuBar project type here:
// https://github.com/microsoft/TemplateStudio/blob/main/docs/WPF/projectTypes/menubar.md
public class ShellViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IRightPaneService _rightPaneService;

    private RelayCommand _goBackCommand;
    private ICommand _menuViewsScreenCaptureCommand;
    private ICommand _menuViewsMonitorCommand;
    private ICommand _menuViewsInRangeCommand;
    private ICommand _menuViewsBlankCommand;
    private ICommand _menuFileSettingsCommand;
    private ICommand _menuViewsMainCommand;
    private ICommand _menuFileExitCommand;
    private ICommand _loadedCommand;
    private ICommand _unloadedCommand;

    //public event PropertyChangedEventHandler WindowsPropertyChanged;

    public RelayCommand GoBackCommand => _goBackCommand ??= new RelayCommand(OnGoBack, CanGoBack);

    public ICommand MenuFileSettingsCommand => _menuFileSettingsCommand ??= new RelayCommand(OnMenuFileSettings);

    public ICommand MenuFileExitCommand => _menuFileExitCommand ??= new RelayCommand(OnMenuFileExit);

    public ICommand LoadedCommand => _loadedCommand ??= new RelayCommand(OnLoaded);

    public ICommand UnloadedCommand => _unloadedCommand ??= new RelayCommand(OnUnloaded);

    public ShellViewModel(INavigationService navigationService, IRightPaneService rightPaneService)
    {
        _navigationService = navigationService;
        _rightPaneService = rightPaneService;
    }



    private void OnLoaded()
    {
        _navigationService.Navigated += OnNavigated;
    }

    private void OnUnloaded()
    {
        _rightPaneService.CleanUp();
        _navigationService.Navigated -= OnNavigated;
    }

    private bool CanGoBack()
        => _navigationService.CanGoBack;

    private void OnGoBack()
        => _navigationService.GoBack();

    private void OnNavigated(object sender, string viewModelName)
    {
        GoBackCommand.NotifyCanExecuteChanged();
    }

    private void OnMenuFileExit()
        => Application.Current.Shutdown();

    public ICommand MenuViewsMainCommand => _menuViewsMainCommand ??= new RelayCommand(OnMenuViewsMain);

    private void OnMenuViewsMain()
        => _navigationService.NavigateTo(typeof(MainViewModel).FullName, null, true);

    private void OnMenuFileSettings()
        => _rightPaneService.OpenInRightPane(typeof(SettingsViewModel).FullName);

    public ICommand MenuViewsBlankCommand => _menuViewsBlankCommand ??= new RelayCommand(OnMenuViewsBlank);

    private void OnMenuViewsBlank()
        => _navigationService.NavigateTo(typeof(BlankViewModel).FullName, null, true);

    public ICommand MenuViewsInRangeCommand => _menuViewsInRangeCommand ??= new RelayCommand(OnMenuViewsInRange);

    private void OnMenuViewsInRange()
        => _navigationService.NavigateTo(typeof(InRangeViewModel).FullName, null, true);

    public ICommand MenuViewsMonitorCommand => _menuViewsMonitorCommand ??= new RelayCommand(OnMenuViewsMonitor);

    private void OnMenuViewsMonitor()
        => _navigationService.NavigateTo(typeof(MonitorViewModel).FullName, null, true);

    public ICommand MenuViewsScreenCaptureCommand => _menuViewsScreenCaptureCommand ??= new RelayCommand(OnMenuViewsScreenCapture);

    private void OnMenuViewsScreenCapture()
        => _navigationService.NavigateTo(typeof(ScreenCaptureViewModel).FullName, null, true);

    //protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    //{
    //    base.OnPropertyChanged(e);

    //    WindowsPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.PropertyName));
    //}
}
