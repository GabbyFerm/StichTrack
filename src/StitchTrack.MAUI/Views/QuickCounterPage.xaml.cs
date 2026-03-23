using CommunityToolkit.Maui.Views;
using StitchTrack.Application.ViewModels;
using StitchTrack.Domain.Interfaces;
using StitchTrack.MAUI.Controls;

namespace StitchTrack.MAUI.Views;

public partial class QuickCounterPage : ContentPage
{
    private readonly QuickCounterViewModel _viewModel;
    private readonly IAppSettingsRepository _appSettingsRepository;

    public QuickCounterPage(
        QuickCounterViewModel viewModel,
        IAppSettingsRepository appSettingsRepository)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _appSettingsRepository = appSettingsRepository;
        BindingContext = _viewModel;
    }

    /// <summary>
    /// Check if this is first launch and show onboarding popup.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            // Small delay for page to settle before showing popup
            await Task.Delay(300);

            var settings = await _appSettingsRepository.GetAppSettingsAsync();
            if (settings?.IsFirstRun == true)
            {
                System.Diagnostics.Debug.WriteLine("🎉 First run detected - showing onboarding");
                var popup = new OnboardingPopup(_appSettingsRepository);
                await this.ShowPopupAsync(popup);
                System.Diagnostics.Debug.WriteLine("✅ Onboarding popup closed");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("✅ Not first run - skipping onboarding");
            }
        }
        catch (InvalidOperationException ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error checking first run: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Popup cancelled: {ex.Message}");
        }
    }

    /// <summary>
    /// Sync icon tapped — Phase 3 placeholder.
    /// </summary>
    private async void OnSyncTapped(object sender, TappedEventArgs e)
    {
        await DisplayAlert("Sync", "Cloud sync coming in Phase 3!", "OK");
    }
}
