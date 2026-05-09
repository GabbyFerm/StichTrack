namespace StitchTrack.Application.Interfaces;

/// <summary>
/// Defines export operations for project data.
/// Implementation lives in the MAUI layer where file system and share sheet APIs are available.
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Exports all projects to a JSON file and opens the system share sheet.
    /// </summary>
    /// <param name="includeArchived">Whether to include archived projects in the export.</param>
    Task ExportJsonAsync(bool includeArchived = false);

    /// <summary>
    /// Exports all projects to a CSV file and opens the system share sheet.
    /// </summary>
    /// <param name="includeArchived">Whether to include archived projects in the export.</param>
    Task ExportCsvAsync(bool includeArchived = false);
}
