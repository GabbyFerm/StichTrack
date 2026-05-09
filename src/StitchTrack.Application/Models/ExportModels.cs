namespace StitchTrack.Application.Models;

/// <summary>
/// Root export document — wraps all projects with metadata.
/// Serialized directly to JSON.
/// </summary>
public class StitchTrackExport
{
    public string ExportedAt { get; init; } = string.Empty;
    public string Version { get; init; } = "1.0";
    public int TotalProjects { get; init; }
    public IReadOnlyList<ProjectExportDto> Projects { get; init; } = new List<ProjectExportDto>();
}

/// <summary>
/// Project data for export — flat representation with sessions included.
/// Excludes cloud sync fields and internal EF navigation properties.
/// </summary>
public class ProjectExportDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int CurrentCount { get; init; }
    public int? TotalRows { get; init; }
    public int? RowsPerRepeat { get; init; }
    public string? Notes { get; init; }
    public string? ColorHex { get; init; }
    public bool IsArchived { get; init; }
    public string CreatedAt { get; init; } = string.Empty;
    public string UpdatedAt { get; init; } = string.Empty;
    public IReadOnlyList<SessionExportDto> Sessions { get; init; } = new List<SessionExportDto>();
}

/// <summary>
/// Session data for export — includes duration and row tracking.
/// </summary>
public class SessionExportDto
{
    public Guid Id { get; init; }
    public string StartedAt { get; init; } = string.Empty;
    public string? EndedAt { get; init; }
    public int DurationSeconds { get; init; }
    public int? StartingRowCount { get; init; }
    public int? EndingRowCount { get; init; }
    public int? RowsCompleted { get; init; }
}
