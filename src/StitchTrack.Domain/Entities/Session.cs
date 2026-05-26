namespace StitchTrack.Domain.Entities;

/// <summary>
/// Represents a work session on a project.
/// Tracks start time, end time, and duration.
/// PrimaryCounterName records which counter was being tracked
/// so the session history can show the correct counter label.
/// </summary>
public class Session
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public int DurationSeconds { get; private set; }

    public int? StartingRowCount { get; private set; }
    public int? EndingRowCount { get; private set; }

    // Name of the primary counter at session start — used for display in session history.
    // Nullable for backward compatibility with sessions recorded before Phase 4.
    // Falls back to "Rows" in the UI when null.
    public string? PrimaryCounterName { get; private set; }

    // Computed properties
    public bool IsActive => !EndedAt.HasValue;

    public int? RowsCompleted =>
        StartingRowCount.HasValue && EndingRowCount.HasValue
            ? EndingRowCount.Value - StartingRowCount.Value
            : null;

    private Session() { }

    /// <summary>
    /// Starts a new session. primaryCounterName is the name of the
    /// primary counter (SortOrder == 0) at the time the session starts.
    /// </summary>
    public static Session StartSession(
        Guid projectId,
        int? startingRowCount = null,
        string? primaryCounterName = null)
    {
        return new Session
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            StartedAt = DateTime.UtcNow,
            EndedAt = null,
            DurationSeconds = 0,
            StartingRowCount = startingRowCount,
            PrimaryCounterName = primaryCounterName
        };
    }

    public void EndSession(int? endingRowCount = null)
    {
        if (EndedAt.HasValue)
            throw new InvalidOperationException("Session is already ended");

        EndedAt = DateTime.UtcNow;
        DurationSeconds = (int)(EndedAt.Value - StartedAt).TotalSeconds;
        EndingRowCount = endingRowCount;
    }
}
