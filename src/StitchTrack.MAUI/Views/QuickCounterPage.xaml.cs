using CommunityToolkit.Maui.Views;
using StitchTrack.Application.ViewModels;
using StitchTrack.MAUI.Controls;

namespace StitchTrack.MAUI.Views;

public partial class QuickCounterPage : ContentPage
{
    private readonly QuickCounterViewModel _viewModel;
    private System.Timers.Timer? _sessionTimer;
    private TimeSpan _sessionDuration;
    private bool _isSessionRunning;

    public QuickCounterPage(QuickCounterViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;

        // Check if onboarding should be shown
        CheckAndShowOnboarding();
    }

    /// <summary>
    /// Check if this is first launch and show onboarding popup.
    /// </summary>
    private async void CheckAndShowOnboarding()
    {
        // TODO: Check AppSettings for HasSeenOnboarding
        // For now, always show on first page load (you'll wire up the repo check)

        bool hasSeenOnboarding = false; // TODO: Load from AppSettingsRepository

        if (!hasSeenOnboarding)
        {
            await Task.Delay(300); // Small delay for page to settle

            var popup = new OnboardingPopup();
            await this.ShowPopupAsync(popup);

            // TODO: Save HasSeenOnboarding = true to AppSettingsRepository
        }
    }

    /// <summary>
    /// Session Play/Pause button clicked.
    /// </summary>
    private void OnSessionButtonClicked(object sender, EventArgs e)
    {
        if (_isSessionRunning)
        {
            // Pause session
            PauseSession();
        }
        else
        {
            // Start/Resume session
            StartSession();
        }
    }

    /// <summary>
    /// Start the session timer.
    /// </summary>
    private void StartSession()
    {
        _isSessionRunning = true;
        SessionButton.Text = "⏸ PAUSE";

        // Create timer if not exists
        if (_sessionTimer == null)
        {
            _sessionTimer = new System.Timers.Timer(1000); // Update every second
            _sessionTimer.Elapsed += OnSessionTimerTick;
        }

        _sessionTimer.Start();
    }

    /// <summary>
    /// Pause the session timer.
    /// </summary>
    private void PauseSession()
    {
        _isSessionRunning = false;
        SessionButton.Text = "▶ PLAY";
        _sessionTimer?.Stop();
    }

    /// <summary>
    /// Session timer tick - update UI every second.
    /// </summary>
    private void OnSessionTimerTick(object? sender, System.Timers.ElapsedEventArgs e)
    {
        _sessionDuration = _sessionDuration.Add(TimeSpan.FromSeconds(1));

        // Update UI on main thread
        MainThread.BeginInvokeOnMainThread(() =>
        {
            SessionTimerLabel.Text = FormatDuration(_sessionDuration);
        });
    }

    /// <summary>
    /// Format duration as "Xh Ym" or "Xm Ys".
    /// </summary>
    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalHours >= 1)
        {
            return $"{(int)duration.TotalHours}h {duration.Minutes}m";
        }
        else if (duration.TotalMinutes >= 1)
        {
            return $"{duration.Minutes}m {duration.Seconds}s";
        }
        else
        {
            return $"{duration.Seconds}s";
        }
    }

    /// <summary>
    /// Sync icon tapped (placeholder).
    /// </summary>
    private async void OnSyncTapped(object sender, TappedEventArgs e)
    {
        // TODO: Implement cloud sync when Phase 3
        await DisplayAlert("Sync", "Cloud sync coming in Phase 3!", "OK");
    }

    /// <summary>
    /// Cleanup timer when page disappears.
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _sessionTimer?.Stop();
        _sessionTimer?.Dispose();
    }
}
