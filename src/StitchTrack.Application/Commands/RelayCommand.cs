using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace StitchTrack.Application.Commands;

/// Simple implementation of ICommand for ViewModel command bindings.
/// Executes an Action when the command is invoked.
public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public event EventHandler? CanExecuteChanged;

    // Creates a command that always executes
    public RelayCommand(Action execute) : this(execute, null)
    {
    }

    // Creates a command with execution logic and optional can-execute check
    public RelayCommand(Action execute, Func<bool>? canExecute)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecute == null || _canExecute();
    }

    public void Execute(object? parameter)
    {
        _execute();
    }

    // Raises CanExecuteChanged to re-evaluate if command can execute
    [SuppressMessage("Design", "CA1030:Use events where appropriate",
        Justification = "This method raises an event, following the standard ICommand pattern")]
    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
