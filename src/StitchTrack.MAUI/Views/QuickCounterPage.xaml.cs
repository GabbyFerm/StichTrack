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
    /// Checks if this is the first app launch and shows the onboarding popup if needed.
    /// Small delay allows the page to render before the popup appears.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            // Small delay for page to settle before showing popup
            await Task.Delay(300).ConfigureAwait(true);

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
}
