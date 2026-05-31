// Copyright (c) 2026 Gabriella Frank Ferm / Frank Ferm Design. All rights reserved.
namespace StitchTrack.Domain.Entities;

/// <summary>
/// Tracks a single counter change for undo functionality.
/// Owned by a ProjectCounter, not directly by Project.
/// In practice, entries are built up in memory during a counting session
/// and used for per-counter undo — they are not persisted during active counting.
/// </summary>
public class CounterHistory
{
    public Guid Id { get; private set; }

    // FK to the specific counter this change belongs to
    public Guid ProjectCounterId { get; private set; }
    public ProjectCounter ProjectCounter { get; private set; } = null!;

    public int OldValue { get; private set; }
    public int NewValue { get; private set; }
    public DateTime ChangedAt { get; private set; }

    private CounterHistory() { }

    /// <summary>
    /// Creates a history entry for a counter change.
    /// Called internally by ProjectCounter — not for direct use.
    /// </summary>
    internal static CounterHistory CreateCounterHistory(
        Guid projectCounterId, int oldValue, int newValue)
    {
        return new CounterHistory
        {
            Id = Guid.NewGuid(),
            ProjectCounterId = projectCounterId,
            OldValue = oldValue,
            NewValue = newValue,
            ChangedAt = DateTime.UtcNow
        };
    }
}
