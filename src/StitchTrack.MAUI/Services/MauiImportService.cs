using StitchTrack.Application.Interfaces;
using StitchTrack.Application.Models;
using StitchTrack.Domain.Entities;
using StitchTrack.Domain.Interfaces;
using System.Text.Json;

namespace StitchTrack.MAUI.Services;

/// <summary>
/// MAUI implementation of IImportService.
/// Opens a file picker, reads the JSON, and creates new projects from the data.
/// File paths from the export are skipped — they are device-specific and invalid on import.
/// </summary>
public class MauiImportService : IImportService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectCounterRepository _counterRepository;
    private readonly ISessionRepository _sessionRepository;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public MauiImportService(
        IProjectRepository projectRepository,
        IProjectCounterRepository counterRepository,
        ISessionRepository sessionRepository)
    {
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        _counterRepository = counterRepository ?? throw new ArgumentNullException(nameof(counterRepository));
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
    }

    public async Task<int> ImportJsonAsync()
    {
        // Open file picker — JSON only
        var file = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Select StitchTrack JSON file",
            FileTypes = new FilePickerFileType(JsonFileTypes)
        }).ConfigureAwait(false);

        // User cancelled
        if (file == null) return -1;

        var json = await File.ReadAllTextAsync(file.FullPath).ConfigureAwait(false);

        var export = JsonSerializer.Deserialize<StitchTrackExport>(json, JsonOptions);

        if (export?.Projects == null || export.Projects.Count == 0)
            return 0;

        int imported = 0;

        foreach (var dto in export.Projects)
        {
            try
            {
                await ImportProjectAsync(dto).ConfigureAwait(false);
                imported++;
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                // Skip failed projects — don't abort the whole import
                System.Diagnostics.Debug.WriteLine(
                    $"⚠️ Skipped project '{dto.Name}': {ex.Message}");
            }
        }

        System.Diagnostics.Debug.WriteLine($"✅ Import complete: {imported} projects");
        return imported;
    }

    // ─── Private helpers ─────────────────────────────────────────
    private static readonly Dictionary<DevicePlatform, IEnumerable<string>> JsonFileTypes = new()
    {
        { DevicePlatform.Android, new[] { "application/json" } },
        { DevicePlatform.iOS,     new[] { "public.json" } },
        { DevicePlatform.WinUI,   new[] { ".json" } }
    };
    private async Task ImportProjectAsync(ProjectExportDto dto)
    {
        // Create project — always a new record regardless of original ID
        var project = Project.CreateProject(dto.Name, colorHex: dto.ColorHex);

        project.UpdateProjectDetails(
            colorHex: dto.ColorHex,
            totalRows: dto.TotalRows,
            rowsPerRepeat: dto.RowsPerRepeat,
            notes: dto.Notes,
            needleOrHookSize: dto.NeedleOrHookSize);

        // File paths (ImagePath) are device-specific — skip on import
        if (dto.IsArchived)
            project.ArchiveProject();

        await _projectRepository.AddAsync(project).ConfigureAwait(false);
        await _projectRepository.SaveChangesAsync().ConfigureAwait(false);

        // Tags
        if (dto.Tags.Count > 0)
        {
            await _projectRepository
                .UpdateTagsAsync(project.Id, dto.Tags)
                .ConfigureAwait(false);
            await _projectRepository.SaveChangesAsync().ConfigureAwait(false);
        }

        // Counters — fall back to a default "Rows" counter for old exports without counter data
        await ImportCountersAsync(project.Id, dto).ConfigureAwait(false);

        // Sessions
        await ImportSessionsAsync(project.Id, dto.Sessions).ConfigureAwait(false);
    }

    private async Task ImportCountersAsync(Guid projectId, ProjectExportDto dto)
    {
        var countersToImport = dto.Counters.Count > 0
            ? dto.Counters.OrderBy(c => c.SortOrder).ToList()
            : new List<CounterExportDto>
              {
                  // Old export format — create a single "Rows" counter from CurrentCount
                  new CounterExportDto
                  {
                      Name = "Rows",
                      CurrentCount = dto.CurrentCount,
                      SortOrder = 0
                  }
              };

        foreach (var counterDto in countersToImport)
        {
            var counter = ProjectCounter.Create(
                projectId,
                counterDto.Name,
                counterDto.SortOrder);

            await _counterRepository.AddAsync(counter).ConfigureAwait(false);
            await _counterRepository.SaveChangesAsync().ConfigureAwait(false);

            // Restore the saved count
            if (counterDto.CurrentCount > 0)
            {
                await _counterRepository.UpdateCountAsync(
                    counter.Id,
                    counterDto.CurrentCount,
                    isPrimary: counterDto.SortOrder == 0,
                    projectId: projectId)
                    .ConfigureAwait(false);
            }
        }
    }

    private async Task ImportSessionsAsync(Guid projectId, IReadOnlyList<SessionExportDto> sessions)
    {
        foreach (var sessionDto in sessions)
        {
            // Parse ISO 8601 timestamps — stored as UTC in the export
            if (!DateTime.TryParse(sessionDto.StartedAt, out var startedAt))
                continue;

            DateTime? endedAt = null;

            if (!string.IsNullOrEmpty(sessionDto.EndedAt)
                && DateTime.TryParse(sessionDto.EndedAt, out var parsedEndedAt))
                endedAt = parsedEndedAt;

            var session = Session.ImportSession(
                projectId,
                startedAt,
                endedAt,
                sessionDto.DurationSeconds,
                sessionDto.StartingRowCount,
                sessionDto.EndingRowCount);

            await _sessionRepository.AddAsync(session).ConfigureAwait(false);
        }

        await _sessionRepository.SaveChangesAsync().ConfigureAwait(false);
    }
}
