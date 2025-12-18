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
/// </summary>
public class QuickCounterViewModel : INotifyPropertyChanged
{
    private readonly Project _project;
    private readonly IProjectRepository _projectRepository;
    private readonly IDialogService _dialogService;

    // Undo stack to track actions
    private readonly Stack<CounterAction> _undoStack = new Stack<CounterAction>();
    private const int MaxUndoStackSize = 50; // Limit undo history

    // PropertyChanged event for data binding
    public event PropertyChangedEventHandler? PropertyChanged;

    // Action to trigger haptic feedback (injected from UI layer)
    public Action? TriggerHapticFeedback { get; set; }

    // Current row count displayed to user
    public int CurrentCount => _project.CurrentCount;

    // Whether the Save button should be enabled
    public bool CanSave => CurrentCount > 0;

    // Whether undo is available
    public bool CanUndo => _undoStack.Count > 0;

    // Commands
    public ICommand IncrementCommand { get; }
    public ICommand DecrementCommand { get; }
    public ICommand UndoCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand SaveToProjectCommand { get; }

    // Callback when project is saved (for navigation)
    public Func<Task>? OnProjectSaved { get; set; }

    public QuickCounterViewModel(IProjectRepository projectRepository, IDialogService dialogService)
    {
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        // Create in-memory project (not saved to database yet)
        _project = Project.CreateProject("Quick Counter");

        // Initialize commands
        IncrementCommand = new RelayCommand(OnIncrement);
        DecrementCommand = new RelayCommand(OnDecrement);
        UndoCommand = new RelayCommand(OnUndo);
        ResetCommand = new RelayCommand(OnReset);
        SaveToProjectCommand = new RelayCommand(OnSaveToProject);

        System.Diagnostics.Debug.WriteLine("✅ QuickCounterViewModel created");
    }

    private void OnIncrement()
    {
        _project.IncrementCount();
        AddToUndoStack(CounterAction.Increment);
        TriggerHapticFeedback?.Invoke();
        NotifyCountChanged();
        System.Diagnostics.Debug.WriteLine($"➕ Incremented to {CurrentCount}");
    }

    private void OnDecrement()
    {
        if (CurrentCount > 0)
        {
            _project.DecrementCount();
            AddToUndoStack(CounterAction.Decrement);
            TriggerHapticFeedback?.Invoke();
            NotifyCountChanged();
            System.Diagnostics.Debug.WriteLine($"➖ Decremented to {CurrentCount}");
        }
    }

    private void OnUndo()
    {
        if (_undoStack.Count == 0)
        {
            System.Diagnostics.Debug.WriteLine("⚠️ Nothing to undo");
            return;
        }

        var lastAction = _undoStack.Pop();

        // Reverse the last action
        switch (lastAction)
        {
            case CounterAction.Increment:
                _project.DecrementCount();
                System.Diagnostics.Debug.WriteLine($"↩️ Undid increment, now at {CurrentCount}");
                break;

            case CounterAction.Decrement:
                _project.IncrementCount();
                System.Diagnostics.Debug.WriteLine($"↩️ Undid decrement, now at {CurrentCount}");
                break;

            case CounterAction.Reset:
                // For reset, we need to store the previous count
                // This is handled differently - see OnReset
                break;
        }

        TriggerHapticFeedback?.Invoke();
        NotifyCountChanged();
    }

    private void OnReset()
    {
        if (CurrentCount > 0)
        {
            // Store current count before reset for potential undo
            var previousCount = CurrentCount;
            AddToUndoStack(CounterAction.Reset, previousCount);

            _project.ResetCount();
            TriggerHapticFeedback?.Invoke();
            NotifyCountChanged();
            System.Diagnostics.Debug.WriteLine($"🔄 Reset from {previousCount} to 0");
        }
    }

    private void OnSaveToProject()
    {
        System.Diagnostics.Debug.WriteLine("💾 Save to Project tapped");
        _ = SaveToProjectAsync();
    }

    /// <summary>
    /// Adds an action to the undo stack with size limit.
    /// </summary>
    private void AddToUndoStack(CounterAction action, int? previousValue = null)
    {
        _undoStack.Push(action);

        // Limit stack size to prevent memory issues
        if (_undoStack.Count > MaxUndoStackSize)
        {
            // Convert to list, remove oldest, convert back
            var items = _undoStack.ToList();
            items.RemoveAt(items.Count - 1);
            _undoStack.Clear();
            foreach (var item in items.AsEnumerable().Reverse())
            {
                _undoStack.Push(item);
            }
        }

        OnPropertyChanged(nameof(CanUndo));

        // Refresh command state
        if (UndoCommand is RelayCommand undoCmd)
        {
            undoCmd.RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// Notifies UI of count-related property changes. 
    /// </summary>
    private void NotifyCountChanged()
    {
        OnPropertyChanged(nameof(CurrentCount));
        OnPropertyChanged(nameof(CanSave));
        OnPropertyChanged(nameof(CanUndo));

        // Refresh command state
        if (UndoCommand is RelayCommand undoCmd)
        {
            undoCmd.RaiseCanExecuteChanged();
        }
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

            // Transfer the current count by incrementing
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
            OnPropertyChanged(nameof(CurrentCount));
            OnPropertyChanged(nameof(CanSave));

            // Notify that the project was saved
            if (OnProjectSaved != null)
            {
                System.Diagnostics.Debug.WriteLine("▶️ Executing OnProjectSaved callback...");
                await OnProjectSaved.Invoke().ConfigureAwait(false);
                System.Diagnostics.Debug.WriteLine("✅ OnProjectSaved callback completed");
            }
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
    /// Shows a brief toast message
    /// </summary>
    private async Task ShowToastAsync(string message)
    {
        await _dialogService.ShowToastAsync(message).ConfigureAwait(false);
    }
    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Enum representing counter actions for undo functionality. 
    /// </summary>
    private enum CounterAction
    {
        Increment,
        Decrement,
        Reset
    }
}
