using System.Windows.Input;

namespace StitchTrack.MAUI.Controls;

/// <summary>
/// Onboarding card shown on first app launch.
/// Welcomes users and explains the app's privacy-first approach.
/// </summary>
public partial class OnboardingCard : ContentView
{
    // Command triggered when user taps "Get Started"
    public static readonly BindableProperty GetStartedCommandProperty =
        BindableProperty.Create(
            nameof(GetStartedCommand),
            typeof(ICommand),
            typeof(OnboardingCard));

    // Command triggered when user taps "Enable Backup & Sync"
    public static readonly BindableProperty EnableSyncCommandProperty =
        BindableProperty.Create(
            nameof(EnableSyncCommand),
            typeof(ICommand),
            typeof(OnboardingCard));

    // Command triggered when user taps "Maybe Later"
    public static readonly BindableProperty MaybeLaterCommandProperty =
        BindableProperty.Create(
            nameof(MaybeLaterCommand),
            typeof(ICommand),
            typeof(OnboardingCard));

    public ICommand? GetStartedCommand
    {
        get => (ICommand?)GetValue(GetStartedCommandProperty);
        set => SetValue(GetStartedCommandProperty, value);
    }

    public ICommand? EnableSyncCommand
    {
        get => (ICommand?)GetValue(EnableSyncCommandProperty);
        set => SetValue(EnableSyncCommandProperty, value);
    }

    public ICommand? MaybeLaterCommand
    {
        get => (ICommand?)GetValue(MaybeLaterCommandProperty);
        set => SetValue(MaybeLaterCommandProperty, value);
    }

    public OnboardingCard()
    {
        InitializeComponent();
    }
}
