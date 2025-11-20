namespace StitchTrack.Domain.Entities;

/// <summary>
/// Represents a work session on a project.
/// Tracks start time, end time, and duration for time management.
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

    // Computed properties
    public bool IsActive => !EndedAt.HasValue;

    public int? RowsCompleted =>
        StartingRowCount.HasValue && EndingRowCount.HasValue
            ? EndingRowCount.Value - StartingRowCount.Value
            : null;

    private Session() { }

    // Starts a new session for a project.
    public static Session StartSession(Guid projectId, int? startingRowCount = null)
    {
        return new Session
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            StartedAt = DateTime.UtcNow,
            EndedAt = null,
            DurationSeconds = 0,
            StartingRowCount = startingRowCount
        };
    }

    // Ends the session and calculates duration.
    public void EndSession(int? endingRowCount = null)
    {
        if (EndedAt.HasValue)
        {
            throw new InvalidOperationException("Session is already ended");
        }

        EndedAt = DateTime.UtcNow;
        DurationSeconds = (int)(EndedAt.Value - StartedAt).TotalSeconds;
        EndingRowCount = endingRowCount;
    }
}
