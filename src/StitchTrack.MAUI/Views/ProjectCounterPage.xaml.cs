using StitchTrack.Application.ViewModels;

namespace StitchTrack.MAUI.Views;

/// <summary>
/// Code-behind for ProjectCounterPage.
/// Owns the session timer — the ViewModel handles all business logic.
/// Timer tick updates the ViewModel which updates the UI via binding.
/// </summary>
[QueryProperty(nameof(ProjectId), "ProjectId")]
// Timer is disposed in OnDisappearing — MAUI pages use lifecycle methods
// instead of IDisposable since the framework controls their lifetime
#pragma warning disable CA1001
public partial class ProjectCounterPage : ContentPage
#pragma warning restore CA1001
{
    private readonly ProjectCounterViewModel _viewModel;
    private string _projectId = string.Empty;

    // Timer lives here (MAUI layer) — ViewModel stays free of System.Timers
    private System.Timers.Timer? _sessionTimer;
    private TimeSpan _sessionDuration;

    private bool _isHandlingBackPress;

    public ProjectCounterPage(ProjectCounterViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = _viewModel;

        // Watch for session start/stop so we can run/stop the timer
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        System.Diagnostics.Debug.WriteLine("✅ ProjectCounterPage initialized");
    }

    public string ProjectId
    {
        get => _projectId;
        set
        {
            _projectId = value;
            if (Guid.TryParse(value, out var projectId))
            {
                _viewModel.ProjectId = projectId;
                System.Diagnostics.Debug.WriteLine($"📌 ProjectId set: {projectId}");
            }
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadProjectAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Always clean up the timer when leaving the page
        StopTimer();
        _sessionTimer?.Dispose();
        _sessionTimer = null;

        // Auto-save count silently when leaving — covers swipe back gesture
        _ = _viewModel.AutoSaveAsync();
    }

    protected override bool OnBackButtonPressed()
    {
        if (_viewModel.IsSessionRunning && !_isHandlingBackPress)
        {
            _isHandlingBackPress = true;
            _ = MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    var result = await DisplayAlert(
                        "Active Session",
                        "You have an active session. Save your progress before leaving?",
                        "Save & Leave",
                        "Leave Without Saving");

                    if (result)
                        await _viewModel.SaveProgressAsync();

                    await Shell.Current.GoToAsync("..");
                }
                finally
                {
                    _isHandlingBackPress = false;
                }
            });
            return true;
        }
        return base.OnBackButtonPressed();
    }

    /// <summary>
    /// Watches IsSessionRunning on the ViewModel and starts/stops the timer accordingly.
    /// Keeps timer lifecycle in the Page where it belongs.
    /// </summary>
    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ProjectCounterViewModel.IsSessionRunning)) return;

        if (_viewModel.IsSessionRunning)
            StartTimer();
        else
            StopTimer();
    }

    private void StartTimer()
    {
        if (_sessionTimer == null)
        {
            _sessionTimer = new System.Timers.Timer(1000);
            _sessionTimer.Elapsed += OnTimerTick;
        }

        _sessionTimer.Start();
        System.Diagnostics.Debug.WriteLine("⏱️ Session timer started");
    }

    private void StopTimer()
    {
        _sessionTimer?.Stop();
        System.Diagnostics.Debug.WriteLine("⏹️ Session timer stopped");
    }

    /// <summary>
    /// Fires every second — updates the ViewModel with the new elapsed time.
    /// ViewModel formats and notifies the UI via binding.
    /// </summary>
    private void OnTimerTick(object? sender, System.Timers.ElapsedEventArgs e)
    {
        _sessionDuration = _sessionDuration.Add(TimeSpan.FromSeconds(1));

        // ViewModel update must happen on the main thread
        MainThread.BeginInvokeOnMainThread(() =>
            _viewModel.UpdateSessionTimer(_sessionDuration)
        );
    }
}
