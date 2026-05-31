using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using StitchTrack.Application.Commands;
using StitchTrack.Application.Interfaces;
using StitchTrack.Domain.Entities;
using StitchTrack.Domain.Interfaces;

namespace StitchTrack.Application.ViewModels;

/// <summary>
/// ViewModel for the Quick Counter — temporary in-memory counting
/// that can optionally be saved as a permanent project.
/// Uses a plain integer for counting rather than a Project entity,
/// since no persistence is needed until the user explicitly saves.
/// </summary>

public class QuickCounterViewModel : INotifyPropertyChanged
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectCounterRepository _counterRepository;
    private readonly IDialogService _dialogService;
    private readonly IHapticsService _hapticsService;

    // Plain integer — no domain entity needed for a temporary counter
    private int _count;

    private readonly Stack<CounterAction> _undoStack = new();
    private const int MaxUndoStackSize = 50;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int CurrentCount => _count;
    public bool CanSave => _count > 0;
    public bool CanUndo => _undoStack.Count > 0;

    public ICommand IncrementCommand { get; }
    public ICommand DecrementCommand { get; }
    public ICommand UndoCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand SaveToProjectCommand { get; }

    /// <summary>
    /// Set by QuickCounterPage — invoked after a successful save to project.
    /// </summary>
    public Func<Task>? OnProjectSaved { get; set; }

    public QuickCounterViewModel(
    IProjectRepository projectRepository,
    IProjectCounterRepository counterRepository,
    IDialogService dialogService,
    IHapticsService hapticsService)
    {
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        _counterRepository = counterRepository ?? throw new ArgumentNullException(nameof(counterRepository));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _hapticsService = hapticsService ?? throw new ArgumentNullException(nameof(hapticsService));

        IncrementCommand = new RelayCommand(OnIncrement);
        DecrementCommand = new RelayCommand(OnDecrement);
        UndoCommand = new RelayCommand(OnUndo);
        ResetCommand = new RelayCommand(OnReset);
        SaveToProjectCommand = new RelayCommand(OnSaveToProject);

        System.Diagnostics.Debug.WriteLine("✅ QuickCounterViewModel created");
    }

    // ─── Counter actions ──────────────────────────────────────────

    private void OnIncrement()
    {
        _count++;
        AddToUndoStack(CounterAction.Increment);
        _hapticsService.Click();
        NotifyCountChanged();
        System.Diagnostics.Debug.WriteLine($"➕ Incremented to {_count}");
    }

    private void OnDecrement()
    {
        if (_count <= 0) return;

        _count--;
        AddToUndoStack(CounterAction.Decrement);
        _hapticsService.Click();
        NotifyCountChanged();
        System.Diagnostics.Debug.WriteLine($"➖ Decremented to {_count}");
    }

    private void OnUndo()
    {
        if (_undoStack.Count == 0) return;

        var lastAction = _undoStack.Pop();

        switch (lastAction)
        {
            case CounterAction.Increment:
                if (_count > 0) _count--;
                System.Diagnostics.Debug.WriteLine($"↩️ Undid increment, now at {_count}");
                break;

            case CounterAction.Decrement:
                _count++;
                System.Diagnostics.Debug.WriteLine($"↩️ Undid decrement, now at {_count}");
                break;

            case CounterAction.Reset:
                // Reset undo not supported — stack entry is a no-op here
                break;
        }

        NotifyCountChanged();
    }

    private void OnReset()
    {
        if (_count <= 0) return;

        AddToUndoStack(CounterAction.Reset);
        _count = 0;
        _hapticsService.Click();
        NotifyCountChanged();
        System.Diagnostics.Debug.WriteLine("🔄 Counter reset to 0");
    }

    private void OnSaveToProject() => _ = SaveToProjectAsync();

    // ─── Save to project ──────────────────────────────────────────

    private async Task SaveToProjectAsync()
    {
        try
        {
            var projectName = await _dialogService.ShowPromptAsync(
                title: "Save to Project",
                message: "Enter a name for this project:",
                accept: "Save",
                cancel: "Cancel",
                placeholder: "My Knitting Project",
                maxLength: 200)
                .ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(projectName))
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Project save cancelled");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"💾 Creating project: {projectName}");

            // Create and save the project
            var newProject = Project.CreateProject(projectName.Trim());
            await _projectRepository.AddAsync(newProject).ConfigureAwait(false);
            await _projectRepository.SaveChangesAsync().ConfigureAwait(false);

            // Create a default "Rows" counter with the current quick count
            var counter = ProjectCounter.Create(newProject.Id, "Rows", sortOrder: 0);
            for (int i = 0; i < _count; i++)
                counter.Increment();  // sets in-memory count + builds undo history

            await _counterRepository.AddAsync(counter).ConfigureAwait(false);
            await _counterRepository.SaveChangesAsync().ConfigureAwait(false);

            // Sync Project.CurrentCount so the project card shows the right value
            await _counterRepository.UpdateCountAsync(
                counter.Id, counter.CurrentCount, isPrimary: true, projectId: newProject.Id)
                .ConfigureAwait(false);

            System.Diagnostics.Debug.WriteLine($"✅ Project saved: {newProject.Id}");

            await _dialogService
                .ShowToastAsync($"'{projectName}' saved!")
                .ConfigureAwait(false);

            // Reset quick counter for next use
            _count = 0;
            _undoStack.Clear();
            NotifyCountChanged();

            if (OnProjectSaved != null)
                await OnProjectSaved.Invoke().ConfigureAwait(false);
        }
        catch (InvalidOperationException ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ DB error: {ex.Message}");
            await _dialogService.ShowAlertAsync("Save Failed", "Could not save project.").ConfigureAwait(false);
        }
        catch (ArgumentException ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Validation: {ex.Message}");
            await _dialogService.ShowAlertAsync("Invalid Input", ex.Message).ConfigureAwait(false);
        }
    }

    // ─── Helpers ─────────────────────────────────────────────────

    private void AddToUndoStack(CounterAction action)
    {
        _undoStack.Push(action);

        if (_undoStack.Count > MaxUndoStackSize)
        {
            var items = _undoStack.ToList();
            items.RemoveAt(items.Count - 1);
            _undoStack.Clear();
            foreach (var item in items.AsEnumerable().Reverse())
                _undoStack.Push(item);
        }

        OnPropertyChanged(nameof(CanUndo));
    }

    private void NotifyCountChanged()
    {
        OnPropertyChanged(nameof(CurrentCount));
        OnPropertyChanged(nameof(CanSave));
        OnPropertyChanged(nameof(CanUndo));
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private enum CounterAction { Increment, Decrement, Reset }

}
