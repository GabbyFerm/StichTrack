namespace StitchTrack.Domain.Entities;

/// <summary>
/// Tracks changes to project counter for undo functionality.
/// Each entry represents a single increment, decrement, or reset action.
/// </summary>
public class CounterHistory
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    public int OldValue { get; private set; }
    public int NewValue { get; private set; }
    public DateTime ChangedAt { get; private set; }

    private CounterHistory() { }

    /// <summary>
    /// Creates a new counter history entry to track a counter change.
    /// Called internally by the Project entity when the counter is modified.
    /// </summary>
    /// <param name="projectId">The project this change belongs to</param>
    /// <param name="oldValue">The counter value before the change</param>
    /// <param name="newValue">The counter value after the change</param>
    /// <returns>A new CounterHistory instance ready for persistence</returns>
    internal static CounterHistory CreateCounterHistory(Guid projectId, int oldValue, int newValue)
    {
        return new CounterHistory
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            OldValue = oldValue,
            NewValue = newValue,
            ChangedAt = DateTime.UtcNow
        };
    }
}
