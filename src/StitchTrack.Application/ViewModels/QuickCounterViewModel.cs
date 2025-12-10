using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using StitchTrack.Application.Commands;
using StitchTrack.Application.Interfaces;
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
    private readonly IProjectRepository _projectRepository;
    private readonly IDialogService _dialogService;
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

    // Whether the Save button should be enabled
    public bool CanSave => CurrentCount > 0;

    // Commands
    public ICommand IncrementCommand { get; }
    public ICommand DecrementCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand GetStartedCommand { get; }
    public ICommand EnableSyncCommand { get; }
    public ICommand MaybeLaterCommand { get; }
    public ICommand SaveToProjectCommand { get; }

    public QuickCounterViewModel(
        IAppSettingsRepository settingsRepository,
        IProjectRepository projectRepository,
        IDialogService dialogService)
    {
        _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

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
        SaveToProjectCommand = new RelayCommand(OnSaveToProject);

        System.Diagnostics.Debug.WriteLine("✅ QuickCounterViewModel created, commands initialized");

        // Ensure initial CanSave state is propagated
        UpdateOnUiThread(() => OnPropertyChanged(nameof(CanSave)));

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
        OnPropertyChanged(nameof(CanSave));
    }

    private void OnDecrement()
    {
        _project.DecrementCount();
        TriggerHapticFeedback?.Invoke();
        OnPropertyChanged(nameof(CurrentCount));
        OnPropertyChanged(nameof(CanSave));
    }

    private void OnReset()
    {
        _project.ResetCount();
        TriggerHapticFeedback?.Invoke();
        OnPropertyChanged(nameof(CurrentCount));
        OnPropertyChanged(nameof(CanSave));
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

    private void OnSaveToProject()
    {
        System.Diagnostics.Debug.WriteLine("💾 Save to Project tapped");
        _ = SaveToProjectAsync();
    }

    /// <summary>
    /// Saves the current quick counter as a permanent project.
    /// Shows a prompt for project name, validates input, and saves to database.
    /// </summary>
    private async Task SaveToProjectAsync()
    {
        try
        {
            // Show project name dialog
            var projectName = await ShowProjectNameDialogAsync().ConfigureAwait(false);

            // Validate name
            if (string.IsNullOrWhiteSpace(projectName))
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Project save cancelled - no name provided");
                return;
            }

            if (projectName.Length > 200)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Project name too long: {projectName.Length} chars");
                await ShowAlertAsync("Name Too Long", "Project name must be 200 characters or less.").ConfigureAwait(false);
                return;
            }

            System.Diagnostics.Debug.WriteLine($"💾 Creating project: {projectName}");

            // Create new project with current counter state
            var newProject = Project.CreateProject(projectName.Trim());

            // Transfer the current count
            // Set the counter to match current count by incrementing
            for (int i = 0; i < _project.CurrentCount; i++)
            {
                newProject.IncrementCount();
            }

            System.Diagnostics.Debug.WriteLine($"✅ Project created with count: {newProject.CurrentCount}");

            // Save to database
            await _projectRepository.AddAsync(newProject).ConfigureAwait(false);
            await _projectRepository.SaveChangesAsync().ConfigureAwait(false);

            System.Diagnostics.Debug.WriteLine($"✅ Project saved to database: {newProject.Id}");

            // Show success message
            await ShowToastAsync($"✅ Project '{projectName}' saved!").ConfigureAwait(false);

            // Reset quick counter for next use
            _project.ResetCount();
            UpdateOnUiThread(() =>
            {
                OnPropertyChanged(nameof(CurrentCount));
                OnPropertyChanged(nameof(CanSave));
            });

            // Navigate to Projects tab (Phase 1: just show message for now)
            System.Diagnostics.Debug.WriteLine("📱 TODO: Navigate to Projects tab");
        }
        catch (InvalidOperationException ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Database error saving project: {ex.Message}");
            await ShowAlertAsync("Save Failed", "Could not save project. Please try again.").ConfigureAwait(false);
        }
        catch (ArgumentException ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Validation error: {ex.Message}");
            await ShowAlertAsync("Invalid Input", ex.Message).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Shows a dialog prompting the user to enter a project name.
    /// Returns the entered name or empty string if cancelled.
    /// </summary>
    private async Task<string> ShowProjectNameDialogAsync()
    {
        var result = await _dialogService.ShowPromptAsync(
            title: "Save to Project",
            message: "Enter a name for this project:",
            accept: "Save",
            cancel: "Cancel",
            placeholder: "My Knitting Project",
            maxLength: 200
        ).ConfigureAwait(false);

        return result ?? string.Empty;
    }

    /// <summary>
    /// Shows an alert dialog with title and message.
    /// </summary>
    private async Task ShowAlertAsync(string title, string message)
    {
        await _dialogService.ShowAlertAsync(title, message).ConfigureAwait(false);
    }

    /// <summary>
    /// Shows a brief toast message (simulated with DisplayAlert for Phase 1).
    /// </summary>
    private async Task ShowToastAsync(string message)
    {
        await _dialogService.ShowToastAsync(message).ConfigureAwait(false);
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
