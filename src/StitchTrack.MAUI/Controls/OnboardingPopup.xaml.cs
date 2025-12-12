using CommunityToolkit.Maui.Views;

namespace StitchTrack.MAUI.Controls;

/// <summary>
/// Onboarding popup shown on first app launch.
/// Uses Community Toolkit Popup for clean UX.
/// </summary>
public partial class OnboardingPopup : Popup
{
    public OnboardingPopup()
    {
        InitializeComponent();
    }

    /// <summary>
    /// "Get Started" button clicked - close popup and mark as seen.
    /// </summary>
    private async void OnGetStartedClicked(object sender, EventArgs e)
    {
        await CloseAsync();
    }

    /// <summary>
    /// "Learn more" link tapped (currently hidden, for future use).
    /// </summary>
    private void OnLearnMoreTapped(object sender, TappedEventArgs e)
    {
        Close();
    }
}
