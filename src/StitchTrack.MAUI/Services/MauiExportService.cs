using StitchTrack.Application.Interfaces;
using StitchTrack.Application.Models;
using StitchTrack.Domain.Entities;
using StitchTrack.Domain.Interfaces;
using System.Text;
using System.Text.Json;

namespace StitchTrack.MAUI.Services;

/// <summary>
/// MAUI implementation of IExportService.
/// Handles JSON/CSV serialization, file writing and share sheet.
/// Serialization logic belongs here since System.Text.Json is available cross-layer,
/// but file system and share sheet APIs are MAUI-only.
/// </summary>
public class MauiExportService : IExportService
{
    private readonly IProjectRepository _projectRepository;

    // Shared JSON options — pretty printed for readability
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public MauiExportService(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository
            ?? throw new ArgumentNullException(nameof(projectRepository));
    }

    // ─── JSON Export ─────────────────────────────────────────────

    public async Task<int> ExportJsonAsync(bool includeArchived = false)
    {
        var projects = await _projectRepository
            .GetAllForExportAsync(includeArchived)
            .ConfigureAwait(false);

        var projectList = projects.ToList();

        // Map domain entities to export DTOs — no EF navigation properties in output
        var exportDto = new StitchTrackExport
        {
            ExportedAt = DateTime.UtcNow.ToString("o"),
            Version = "1.0",
            TotalProjects = projectList.Count,
            Projects = projectList.Select(p => new ProjectExportDto
            {
                Id = p.Id,
                Name = p.Name,
                CurrentCount = p.CurrentCount,
                TotalRows = p.TotalRows,
                RowsPerRepeat = p.RowsPerRepeat,
                NeedleOrHookSize = p.NeedleOrHookSize,
                Tags = p.Tags.OrderBy(t => t.ColorIndex).Select(t => t.Name).ToList(),
                Notes = p.Notes,
                ColorHex = p.ColorHex,
                IsArchived = p.IsArchived,
                CreatedAt = p.CreatedAt.ToString("o"),
                UpdatedAt = p.UpdatedAt.ToString("o"),
                Counters = p.Counters
                .OrderBy(c => c.SortOrder)
                .Select(c => new CounterExportDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    CurrentCount = c.CurrentCount,
                    SortOrder = c.SortOrder
                }).ToList(),
                Sessions = p.Sessions.Select(s => new SessionExportDto
                {
                    Id = s.Id,
                    StartedAt = s.StartedAt.ToString("o"),
                    EndedAt = s.EndedAt?.ToString("o"),
                    DurationSeconds = s.DurationSeconds,
                    StartingRowCount = s.StartingRowCount,
                    EndingRowCount = s.EndingRowCount,
                    RowsCompleted = s.RowsCompleted
                }).ToList()
            }).ToList()
        };

        var json = JsonSerializer.Serialize(exportDto, JsonOptions);
        var fileName = $"stitchtrack-export-{DateTime.Now:yyyy-MM-dd}.json";

        await WriteAndShareAsync(fileName, json, "application/json").ConfigureAwait(false);
        System.Diagnostics.Debug.WriteLine($"✅ JSON export complete: {fileName} ({projectList.Count} projects)");

        return projectList.Count;
    }

    // ─── CSV Export ──────────────────────────────────────────────

    public async Task<int> ExportCsvAsync(bool includeArchived = false)
    {
        var projectList = (await _projectRepository
            .GetAllForExportAsync(includeArchived)
            .ConfigureAwait(false)).ToList();

        var csv = new StringBuilder();

        // Header row
        csv.AppendLine("Name,CurrentCount,TotalRows,NeedleOrHookSize,Tags,Notes,CreatedAt,IsArchived");

        // Data rows — escape fields that may contain commas or quotes
        foreach (var project in projectList)
        {
            csv.AppendLine(string.Join(",",
                EscapeCsvField(project.Name),
                project.CurrentCount,
                project.TotalRows?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                EscapeCsvField(project.NeedleOrHookSize ?? string.Empty),
                EscapeCsvField(string.Join(";", project.Tags.OrderBy(t => t.ColorIndex).Select(t => t.Name))),
                EscapeCsvField(project.Notes ?? string.Empty),
                project.CreatedAt.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                project.IsArchived.ToString(System.Globalization.CultureInfo.InvariantCulture)
            ));
        }

        var fileName = $"stitchtrack-export-{DateTime.Now:yyyy-MM-dd}.csv";

        await WriteAndShareAsync(fileName, csv.ToString(), "text/csv").ConfigureAwait(false);
        System.Diagnostics.Debug.WriteLine($"✅ CSV export complete: {fileName} ({projectList.Count} projects)");

        return projectList.Count;
    }

    // ─── Helpers ─────────────────────────────────────────────────

    /// <summary>
    /// Writes content to a temp file in app cache and opens the system share sheet.
    /// </summary>
    private static async Task WriteAndShareAsync(string fileName, string content, string mimeType)
    {
        // Write to app cache directory — accessible by share sheet
        var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
        await File.WriteAllTextAsync(filePath, content, Encoding.UTF8).ConfigureAwait(false);

        System.Diagnostics.Debug.WriteLine($"📁 Export file written: {filePath}");

        // Open share sheet on main thread — UI API
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Export StitchTrack Data",
                File = new ShareFile(filePath, mimeType)
            });
        });
    }

    /// <summary>
    /// Wraps a CSV field in quotes if it contains commas, quotes or newlines.
    /// Escapes internal quotes by doubling them per RFC 4180.
    /// </summary>
    private static string EscapeCsvField(string field)
    {
        if (field.Contains(',', StringComparison.Ordinal) ||
            field.Contains('"', StringComparison.Ordinal) ||
            field.Contains('\n', StringComparison.Ordinal) ||
            field.Contains('\r', StringComparison.Ordinal))
            return $"\"{field.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";

        return field;
    }
}
