using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using StitchTrack.Application.Commands;
using StitchTrack.Domain.Entities;

namespace StitchTrack.Application.ViewModels;

/// ViewModel for guest mode counter - no login required, in-memory only.
/// Wraps a Project entity to provide UI-friendly commands and property notifications.

public class GuestCounterViewModel : INotifyPropertyChanged
{
    private readonly Project _project;

    // PropertyChanged event for data binding
    public event PropertyChangedEventHandler? PropertyChanged;

    // Action to trigger haptic feedback (injected from UI layer)
    public Action? TriggerHapticFeedback { get; set; }

    // Current row count displayed to user
    public int CurrentCount => _project.CurrentCount;

    // Command to increment counter (+1 button)
    public ICommand IncrementCommand { get; }

    // Command to decrement counter (-1 button)
    public ICommand DecrementCommand { get; }

    // Command to reset counter to zero
    public ICommand ResetCommand { get; }

    public GuestCounterViewModel()
    {
        // Create in-memory project (not saved to database)
        _project = Project.Create("Guest Counter");

        // Initialize commands with actions
        IncrementCommand = new RelayCommand(OnIncrement);
        DecrementCommand = new RelayCommand(OnDecrement);
        ResetCommand = new RelayCommand(OnReset);
    }

    private void OnIncrement()
    {
        _project.IncrementCount();
        TriggerHapticFeedback?.Invoke();
        OnPropertyChanged(nameof(CurrentCount)); // Notify UI to update
    }

    private void OnDecrement()
    {
        _project.DecrementCount();
        TriggerHapticFeedback?.Invoke();
        OnPropertyChanged(nameof(CurrentCount)); // Notify UI to update
    }

    private void OnReset()
    {
        _project.ResetCount();
        TriggerHapticFeedback?.Invoke();
        OnPropertyChanged(nameof(CurrentCount)); // Notify UI to update
    }

    // Raises PropertyChanged event to update UI bindings
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
