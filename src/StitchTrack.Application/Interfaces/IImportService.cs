namespace StitchTrack.Application.Interfaces;

/// <summary>
/// Handles importing project data from a JSON file.
/// File picking and parsing are MAUI-specific — implemented in the MAUI layer.
/// </summary>
public interface IImportService
{
    /// <summary>
    /// Opens a file picker, reads the selected JSON file, and creates new projects.
    /// Always creates new projects — never overwrites existing data.
    /// Returns the number of projects imported, or -1 if the user cancelled.
    /// </summary>
    Task<int> ImportJsonAsync();
}
