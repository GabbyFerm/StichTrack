using CommunityToolkit.Maui.Views;
using StitchTrack.Application.ViewModels;
using StitchTrack.Domain.Interfaces;
using StitchTrack.MAUI.Controls;

namespace StitchTrack.MAUI.Views;

public partial class QuickCounterPage : ContentPage
{
    private readonly QuickCounterViewModel _viewModel;
    private readonly IAppSettingsRepository _appSettingsRepository;
    private System.Timers.Timer? _sessionTimer;
    private TimeSpan _sessionDuration;
    private bool _isSessionRunning;

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
            // Check if user has seen onboarding - use correct method name
            var settings = await _appSettingsRepository.GetAppSettingsAsync();
            if (settings?.IsFirstRun == true)
            {
                System.Diagnostics.Debug.WriteLine("🎉 First run detected - showing onboarding");

                // Small delay for page to settle
                await Task.Delay(300);

                // Create and show popup (DI will inject AppSettingsRepository)
                var popup = new OnboardingPopup(_appSettingsRepository);
                await this.ShowPopupAsync(popup);

                System.Diagnostics.Debug.WriteLine("✅ Onboarding popup closed");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("✅ Not first run - skipping onboarding");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error checking first run: {ex.Message}");
            // Don't crash if this fails - just don't show the popup
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
        SessionButton.Text = "⸜ PAUSE";

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
