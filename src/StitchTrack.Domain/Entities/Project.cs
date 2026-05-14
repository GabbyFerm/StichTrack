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

    // Free-text so users can write "5.0mm", "US 8", "4.5mm / G-6", etc.
    public string? NeedleOrHookSize { get; private set; }


    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Cloud sync fields (nullable for local-only projects)
    public DateTime? LastSyncedAt { get; private set; }
    public string? CloudFileId { get; private set; } // iCloud/Drive file ID
    public int SyncVersion { get; private set; } // increment on each change

    public ICollection<Session> Sessions { get; private set; } = new List<Session>();
    public ICollection<CounterHistory> CounterHistoryEntries { get; private set; } = new List<CounterHistory>();
    public ICollection<RowNote> RowNotes { get; private set; } = new List<RowNote>();
    public ICollection<ProjectFile> ProjectFiles { get; private set; } = new List<ProjectFile>();
    public ICollection<Reminder> Reminders { get; private set; } = new List<Reminder>();

    // Tags owned by this project — loaded via .Include(p => p.Tags)
    public ICollection<ProjectTag> Tags { get; private set; } = new List<ProjectTag>();


    private Project() { }

    // Factory method to create a new project with validated initial state
    public static Project CreateProject(string name, Guid? userId = null, string? colorHex = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name cannot be empty", nameof(name));

        var now = DateTime.UtcNow;

        return new Project
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name.Trim(),
            CurrentCount = 0,
            ColorHex = colorHex ?? ProjectColors.GetRandomColor(),
            CreatedAt = now,
            UpdatedAt = now
        };

    }

    /// <summary>
    /// Renames the project. Only the domain entity controls name changes.
    /// </summary>
    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Project name cannot be empty", nameof(newName));

        Name = newName.Trim();
        UpdatedAt = DateTime.UtcNow;
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

    // Only the project controls when history is recorded
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

    /// <summary>
    /// Updates the optional detail fields on the project.
    /// Pass null to clear a field.
    /// </summary>
    public void UpdateProjectDetails(
        string? colorHex = null,
        int? totalRows = null,
        int? rowsPerRepeat = null,
        string? notes = null,
        string? needleOrHookSize = null)
    {
        ColorHex = colorHex;
        TotalRows = totalRows;
        RowsPerRepeat = rowsPerRepeat;
        Notes = notes;
        NeedleOrHookSize = needleOrHookSize;
        UpdatedAt = DateTime.UtcNow;
    }


    public void SetProjectImage(string? imagePath, string? imageUrl = null)
    {
        ImagePath = imagePath;
        ImageUrl = imageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    // ─── Tag management ────────────────────────────────────────────
    // The aggregate root controls all tag mutations so the collection
    // never ends up in an inconsistent state.

    /// <summary>
    /// Adds a tag if one with the same name does not already exist (case-insensitive).
    /// ColorIndex should be the tag's position in the list % TagColors.Palette.Length.
    /// </summary>
    public void AddTag(string name, int colorIndex)
    {
        // Silently ignore duplicate tag names
        if (Tags.Any(t => t.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)))
            return;

        Tags.Add(ProjectTag.Create(Id, name, colorIndex));
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes the tag with the given name (case-insensitive). No-op if not found.
    /// </summary>
    public void RemoveTag(string name)
    {
        var tag = Tags.FirstOrDefault(
            t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (tag != null)
        {
            Tags.Remove(tag);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Removes all tags. Used before re-syncing the full tag list from the form.
    /// </summary>
    public void ClearTags()
    {
        Tags.Clear();
        UpdatedAt = DateTime.UtcNow;
    }


    // ─── Cloud sync ────────────────────────────────────────────────

    public void MarkAsSynced(string cloudFileId)
    {
        if (string.IsNullOrWhiteSpace(cloudFileId))
            throw new ArgumentException("Cloud file ID cannot be empty", nameof(cloudFileId));

        CloudFileId = cloudFileId;
        LastSyncedAt = DateTime.UtcNow;
        SyncVersion++;
    }

}
