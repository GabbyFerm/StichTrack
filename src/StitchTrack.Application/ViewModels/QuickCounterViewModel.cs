using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using StitchTrack.Application.Commands;
using StitchTrack.Domain.Entities;
using StitchTrack.Domain.Interfaces;

namespace StitchTrack.Application.ViewModels;

/// <summary>
/// ViewModel for Quick Counter - temporary counting that can be saved to a project.
/// Manages onboarding display and counter functionality.
/// </summary>
public class QuickCounterViewModel : INotifyPropertyChanged
{
    private readonly Project _project;
    private readonly IAppSettingsRepository _settingsRepository;
    private readonly SynchronizationContext? _syncContext;
    private bool _showOnboarding;

    // PropertyChanged event for data binding
    public event PropertyChangedEventHandler? PropertyChanged;

    // Action to trigger haptic feedback (injected from UI layer)
    public Action? TriggerHapticFeedback { get; set; }

    // Controls whether onboarding card is visible
    public bool ShowOnboarding
    {
        get => _showOnboarding;
        private set
        {
            if (_showOnboarding != value)
            {
                _showOnboarding = value;
                OnPropertyChanged();
                System.Diagnostics.Debug.WriteLine($"🔄 ShowOnboarding changed to: {value}");
            }
        }
    }

    // Current row count displayed to user
    public int CurrentCount => _project.CurrentCount;

    // Commands
    public ICommand IncrementCommand { get; }
    public ICommand DecrementCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand GetStartedCommand { get; }
    public ICommand EnableSyncCommand { get; }
    public ICommand MaybeLaterCommand { get; }

    public QuickCounterViewModel(IAppSettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));

        // Capture the UI synchronization context for cross-thread updates
        _syncContext = SynchronizationContext.Current;

        // Create in-memory project (not saved to database yet)
        _project = Project.CreateProject("Quick Counter");

        // Initialize commands
        IncrementCommand = new RelayCommand(OnIncrement);
        DecrementCommand = new RelayCommand(OnDecrement);
        ResetCommand = new RelayCommand(OnReset);
        GetStartedCommand = new RelayCommand(OnGetStarted);
        EnableSyncCommand = new RelayCommand(OnEnableSync);
        MaybeLaterCommand = new RelayCommand(OnMaybeLater);

        System.Diagnostics.Debug.WriteLine("✅ QuickCounterViewModel created, commands initialized");

        // Check if we should show onboarding
        _ = CheckAndShowOnboardingAsync();
    }

    private async Task CheckAndShowOnboardingAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔍 Checking if should show onboarding...");
            var settings = await _settingsRepository.GetAppSettingsAsync().ConfigureAwait(false);

            System.Diagnostics.Debug.WriteLine($"📋 Settings loaded: IsFirstRun = {settings.IsFirstRun}");

            // Update ShowOnboarding on UI thread
            UpdateOnUiThread(() => ShowOnboarding = settings.IsFirstRun);
        }
        catch (InvalidOperationException ex)
        {
            // Database not initialized yet
            System.Diagnostics.Debug.WriteLine($"⚠️ Database error loading settings: {ex.Message}");
            UpdateOnUiThread(() => ShowOnboarding = false);
        }
        catch (ArgumentException ex)
        {
            // Invalid data in database
            System.Diagnostics.Debug.WriteLine($"⚠️ Invalid settings data: {ex.Message}");
            UpdateOnUiThread(() => ShowOnboarding = false);
        }
    }

    private void OnIncrement()
    {
        _project.IncrementCount();
        TriggerHapticFeedback?.Invoke();
        OnPropertyChanged(nameof(CurrentCount));
    }

    private void OnDecrement()
    {
        _project.DecrementCount();
        TriggerHapticFeedback?.Invoke();
        OnPropertyChanged(nameof(CurrentCount));
    }

    private void OnReset()
    {
        _project.ResetCount();
        TriggerHapticFeedback?.Invoke();
        OnPropertyChanged(nameof(CurrentCount));
    }

    private void OnGetStarted()
    {
        System.Diagnostics.Debug.WriteLine("🚀 Get Started tapped");
        _ = CompleteOnboardingAsync();
    }

    private void OnEnableSync()
    {
        System.Diagnostics.Debug.WriteLine("☁️ Enable Sync tapped - Phase 3 feature");
        _ = CompleteOnboardingAsync();
    }

    private void OnMaybeLater()
    {
        System.Diagnostics.Debug.WriteLine("⏭️ Maybe Later tapped");
        _ = CompleteOnboardingAsync();
    }

    private async Task CompleteOnboardingAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("💾 Completing onboarding...");

            var settings = await _settingsRepository.GetAppSettingsAsync().ConfigureAwait(false);
            settings.CompleteFirstRun();
            await _settingsRepository.SaveAppSettingsAsync(settings).ConfigureAwait(false);

            System.Diagnostics.Debug.WriteLine("✅ Onboarding completed, hiding card");

            // Update UI on main thread
            UpdateOnUiThread(() => ShowOnboarding = false);
        }
        catch (InvalidOperationException ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Database error completing onboarding: {ex.Message}");
            UpdateOnUiThread(() => ShowOnboarding = false); // Hide anyway to not block user
        }
        catch (ArgumentException ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Invalid data completing onboarding: {ex.Message}");
            UpdateOnUiThread(() => ShowOnboarding = false);
        }
    }

    /// <summary>
    /// Executes an action on the UI thread using SynchronizationContext.
    /// Falls back to direct execution if no sync context is available.
    /// </summary>
    private void UpdateOnUiThread(Action action)
    {
        if (_syncContext != null)
        {
            _syncContext.Post(_ => action(), null);
        }
        else
        {
            // Fallback: execute directly (might be in test environment)
            action();
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
