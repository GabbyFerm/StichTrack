namespace StitchTrack.Domain.Entities;

/// <summary>
/// Represents a knitting or crocheting project with row counting capability.
/// Aggregate root for counter history, sessions, notes, and files.
/// </summary>
public class Project
{
    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; }
    public User? User { get; private set; }

    public string Name { get; private set; } = string.Empty;
    public int CurrentCount { get; private set; }

    public string? ColorHex { get; private set; }
    public int? TotalRows { get; private set; }
    public int? RowsPerRepeat { get; private set; }
    public string? Notes { get; private set; }
    public bool IsArchived { get; private set; }
    public string? ImagePath { get; private set; }
    public string? ImageUrl { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public ICollection<Session> Sessions { get; private set; } = new List<Session>();
    public ICollection<CounterHistory> CounterHistoryEntries { get; private set; } = new List<CounterHistory>();
    public ICollection<RowNote> RowNotes { get; private set; } = new List<RowNote>();
    public ICollection<PatternFile> PatternFiles { get; private set; } = new List<PatternFile>();
    public ICollection<Reminder> Reminders { get; private set; } = new List<Reminder>();

    private Project() { }

    // Factory method to create a new project with validated initial state
    public static Project CreateProject(string name, Guid? userId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Project name cannot be empty", nameof(name));
        }

        var now = DateTime.UtcNow;

        return new Project
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name.Trim(),
            CurrentCount = 0,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void IncrementCount()
    {
        int oldValue = CurrentCount;
        CurrentCount++;
        UpdatedAt = DateTime.UtcNow;

        RecordCounterChange(oldValue, CurrentCount);
    }

    public void DecrementCount()
    {
        if (CurrentCount > 0)
        {
            int oldValue = CurrentCount;
            CurrentCount--;
            UpdatedAt = DateTime.UtcNow;

            RecordCounterChange(oldValue, CurrentCount);
        }
    }

    public void ResetCount()
    {
        int oldValue = CurrentCount;
        CurrentCount = 0;
        UpdatedAt = DateTime.UtcNow;

        RecordCounterChange(oldValue, CurrentCount);
    }

    public bool UndoLastChange()
    {
        var lastChange = CounterHistoryEntries
            .OrderByDescending(h => h.ChangedAt)
            .FirstOrDefault();

        if (lastChange == null)
        {
            return false;
        }

        CurrentCount = lastChange.OldValue;
        UpdatedAt = DateTime.UtcNow;

        CounterHistoryEntries.Remove(lastChange);

        return true;
    }

    // Record a counter change in history for undo functionality, only Project should control when history is recorded
    private void RecordCounterChange(int oldValue, int newValue)
    {
        var history = CounterHistory.CreateCounterHistory(Id, oldValue, newValue);
        CounterHistoryEntries.Add(history);
    }

    public void ArchiveProject()
    {
        IsArchived = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnarchiveProject()
    {
        IsArchived = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProjectDetails(string? colorHex = null, int? totalRows = null, int? rowsPerRepeat = null, string? notes = null)
    {
        ColorHex = colorHex;
        TotalRows = totalRows;
        RowsPerRepeat = rowsPerRepeat;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetProjectImage(string? imagePath, string? imageUrl = null)
    {
        ImagePath = imagePath;
        ImageUrl = imageUrl;
        UpdatedAt = DateTime.UtcNow;
    }
}
