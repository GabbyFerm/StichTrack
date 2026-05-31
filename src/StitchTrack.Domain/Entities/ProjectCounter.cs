// Copyright (c) 2026 Gabriella Frank Ferm / Frank Ferm Design. All rights reserved.
namespace StitchTrack.Domain.Entities;

/// <summary>
/// A named counter belonging to a project.
/// Each counter tracks its own count and maintains an in-memory undo history.
/// The primary counter (SortOrder == 0) drives Project.CurrentCount and session tracking.
/// </summary>
public class ProjectCounter
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    public string Name { get; private set; } = string.Empty;
    public int CurrentCount { get; private set; }

    // 0 = primary counter (drives Project.CurrentCount and sessions)
    public int SortOrder { get; private set; }

    public DateTime CreatedAt { get; private set; }

    // In-memory undo history — same pattern as Project.CounterHistoryEntries.
    // Entries are added during a session and used for per-counter undo.
    // Not persisted to the DB during active counting (AsNoTracking on load).
    public ICollection<CounterHistory> CounterHistoryEntries { get; private set; }
        = new List<CounterHistory>();

    private ProjectCounter() { }

    public static ProjectCounter Create(Guid projectId, string name, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Counter name cannot be empty.", nameof(name));

        return new ProjectCounter
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Name = name.Trim(),
            CurrentCount = 0,
            SortOrder = sortOrder,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ─── Counting actions ─────────────────────────────────────────

    public void Increment()
    {
        int old = CurrentCount;
        CurrentCount++;
        RecordChange(old, CurrentCount);
    }

    public void Decrement()
    {
        if (CurrentCount <= 0) return;
        int old = CurrentCount;
        CurrentCount--;
        RecordChange(old, CurrentCount);
    }

    public void Reset()
    {
        int old = CurrentCount;
        CurrentCount = 0;
        RecordChange(old, CurrentCount);
    }

    /// <summary>
    /// Reverts the last change using the in-memory history stack.
    /// Returns true if a change was undone, false if no history exists.
    /// </summary>
    public bool UndoLastChange()
    {
        var last = CounterHistoryEntries
            .OrderByDescending(h => h.ChangedAt)
            .FirstOrDefault();

        if (last == null) return false;

        CurrentCount = last.OldValue;
        CounterHistoryEntries.Remove(last);
        return true;
    }

    // ─── Management ───────────────────────────────────────────────

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Counter name cannot be empty.", nameof(newName));

        Name = newName.Trim();
    }

    // Only the counter controls when history is recorded
    private void RecordChange(int oldValue, int newValue)
    {
        var entry = CounterHistory.CreateCounterHistory(Id, oldValue, newValue);
        CounterHistoryEntries.Add(entry);
    }
}
