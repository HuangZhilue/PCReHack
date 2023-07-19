using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CV_ViewTool.Contracts.Services;
using CV_ViewTool.Contracts.ViewModels;
using CV_ViewTool.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Windows.Input;

namespace CV_ViewTool.ViewModels;

public class SettingsViewModel : ObservableObject, INavigationAware
{
    private AppConfig AppConfig { get; }
    private IThemeSelectorService ThemeSelectorService { get; }
    private ISystemService SystemService { get; }
    private IApplicationInfoService ApplicationInfoService { get; }

    private AppTheme _theme;
    private string _versionDescription;
    private ICommand _setThemeCommand;
    private ICommand _privacyStatementCommand;

    public AppTheme Theme
    {
        get => _theme;
        set { SetProperty(ref _theme, value); }
    }

    public string VersionDescription
    {
        get => _versionDescription;
        set { SetProperty(ref _versionDescription, value); }
    }

    public ICommand SetThemeCommand => _setThemeCommand ??= new RelayCommand<string>(OnSetTheme);

    public ICommand PrivacyStatementCommand => _privacyStatementCommand ??= new RelayCommand(OnPrivacyStatement);

    public SettingsViewModel(IServiceProvider service)
    {
        AppConfig = service.GetRequiredService<IOptions<AppConfig>>().Value;
        ThemeSelectorService = service.GetRequiredService<IThemeSelectorService>();
        SystemService = service.GetRequiredService<ISystemService>();
        ApplicationInfoService = service.GetRequiredService<IApplicationInfoService>();
    }

    public void OnNavigatedTo(object parameter)
    {
        VersionDescription = $"{Properties.Resources.AppDisplayName} - {ApplicationInfoService.GetVersion()}";
        Theme = ThemeSelectorService.GetCurrentTheme();
    }

    public void OnNavigatedFrom()
    {
    }

    private void OnSetTheme(string themeName)
    {
        ThemeSelectorService.SetTheme((AppTheme)Enum.Parse(typeof(AppTheme), themeName));
    }

    private void OnPrivacyStatement()
    {
        SystemService.OpenInWebBrowser(AppConfig.PrivacyStatement);
    }
}
