using CommunityToolkit.Maui.Views;
using StitchTrack.Domain.Interfaces;

namespace StitchTrack.MAUI.Controls;

/// <summary>
/// Onboarding popup shown on first app launch.
/// Uses Community Toolkit Popup for clean UX.
/// Integrates with AppSettings to track HasSeenOnboarding.
/// </summary>
public partial class OnboardingPopup : Popup
{
    private readonly IAppSettingsRepository _appSettingsRepository;

    public OnboardingPopup(IAppSettingsRepository appSettingsRepository)
    {
        InitializeComponent();
        _appSettingsRepository = appSettingsRepository ?? throw new ArgumentNullException(nameof(appSettingsRepository));
    }

    /// <summary>
    /// "Get Started" button clicked - close popup and mark as seen.
    /// </summary>
    private async void OnGetStartedClicked(object sender, EventArgs e)
    {
        await MarkOnboardingAsSeenAsync();
        await CloseAsync();
    }

    /// <summary>
    /// Close button (X) clicked - close popup and mark as seen.
    /// </summary>
    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await MarkOnboardingAsSeenAsync();
        await CloseAsync();
    }

    /// <summary>
    /// Marks onboarding as seen in AppSettings.
    /// Sets IsFirstRun = false so it won't show again.
    /// </summary>
    private async Task MarkOnboardingAsSeenAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("📝 Marking onboarding as seen...");

            // Use correct method name from IAppSettingsRepository
            var settings = await _appSettingsRepository.GetAppSettingsAsync();
            if (settings != null)
            {
                // Mark first run as complete
                settings.CompleteFirstRun();

                // Save changes using correct method name
                await _appSettingsRepository.SaveAppSettingsAsync(settings);

                System.Diagnostics.Debug.WriteLine("✅ Onboarding marked as seen");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error marking onboarding as seen: {ex.Message}");
            // Don't crash the app if this fails - just log it
        }
    }
}
