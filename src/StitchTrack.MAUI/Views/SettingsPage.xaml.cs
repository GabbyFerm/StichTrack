using StitchTrack.Application.ViewModels;

namespace StitchTrack.MAUI.Views;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;

    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = _viewModel;

        // AppInfo is MAUI-only — set it here rather than in the ViewModel
        _viewModel.AppVersion = $"Version {AppInfo.Current.VersionString}";

        _viewModel.ApplyTheme = (theme) =>
        {
            Microsoft.Maui.Controls.Application.Current!.UserAppTheme = theme switch
            {
                "Light" => AppTheme.Light,
                "Dark" => AppTheme.Dark,
                _ => AppTheme.Unspecified
            };
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadSettingsAsync();
    }
}
