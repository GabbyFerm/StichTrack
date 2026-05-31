namespace StitchTrack.Domain.Entities;

/// <summary>
/// Represents a knitting or crocheting project.
/// Aggregate root for sessions, files, tags, row notes, and counters.
///
/// Counting operations have moved to ProjectCounter.
/// Project.CurrentCount is a cached value of the primary counter (SortOrder == 0),
/// kept in sync by ProjectCounterRepository.UpdateCountAsync when isPrimary = true.
/// This cache is used by session tracking, export, and the project list card.
/// </summary>
public class Project
{
    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; }
    public User? User { get; private set; }

    public string Name { get; private set; } = string.Empty;

    // Cached value of the primary counter — kept in sync by the repository.
    public int CurrentCount { get; private set; }

    public string? ColorHex { get; private set; }
    public int? TotalRows { get; private set; }
    public int? RowsPerRepeat { get; private set; }
    public string? Notes { get; private set; }
    public string? NeedleOrHookSize { get; private set; }
    public bool IsArchived { get; private set; }
    public string? ImagePath { get; private set; }
    public string? ImageUrl { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public DateTime? LastSyncedAt { get; private set; }
    public string? CloudFileId { get; private set; }
    public int SyncVersion { get; private set; }

    public ICollection<Session> Sessions { get; private set; } = new List<Session>();
    public ICollection<RowNote> RowNotes { get; private set; } = new List<RowNote>();
    public ICollection<ProjectTag> Tags { get; private set; } = new List<ProjectTag>();
    public ICollection<ProjectFile> ProjectFiles { get; private set; } = new List<ProjectFile>();
    public ICollection<Reminder> Reminders { get; private set; } = new List<Reminder>();
    public ICollection<ProjectCounter> Counters { get; private set; } = new List<ProjectCounter>();

    private Project() { }

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

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Project name cannot be empty", nameof(newName));
        Name = newName.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProjectDetails(
        string? colorHex = null, int? totalRows = null, int? rowsPerRepeat = null,
        string? notes = null, string? needleOrHookSize = null)
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

    public void ArchiveProject() { IsArchived = true; UpdatedAt = DateTime.UtcNow; }
    public void UnarchiveProject() { IsArchived = false; UpdatedAt = DateTime.UtcNow; }

    // ─── Counter management ───────────────────────────────────────

    /// <summary>
    /// Adds a named counter. SortOrder 0 = primary (drives CurrentCount and sessions).
    /// Returns the new counter so the caller can persist it.
    /// </summary>
    public ProjectCounter AddCounter(string name, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Counter name cannot be empty.", nameof(name));

        var counter = ProjectCounter.Create(Id, name.Trim(), sortOrder);
        Counters.Add(counter);
        UpdatedAt = DateTime.UtcNow;
        return counter;
    }

    /// <summary>
    /// Removes a counter by ID.
    /// Cascade delete of its history is handled at the DB level.
    /// </summary>
    public void RemoveCounter(Guid counterId)
    {
        var counter = Counters.FirstOrDefault(c => c.Id == counterId);
        if (counter != null)
        {
            Counters.Remove(counter);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    // ─── Tag management ────────────────────────────────────────────

    public void AddTag(string name, int colorIndex)
    {
        if (Tags.Any(t => t.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)))
            return;
        Tags.Add(ProjectTag.Create(Id, name, colorIndex));
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveTag(string name)
    {
        var tag = Tags.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (tag != null) { Tags.Remove(tag); UpdatedAt = DateTime.UtcNow; }
    }

    public void ClearTags() { Tags.Clear(); UpdatedAt = DateTime.UtcNow; }

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
