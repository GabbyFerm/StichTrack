namespace StitchTrack.Application.Models;

/// <summary>
/// Carries the result of the project create/edit form popup back to the ViewModel.
/// Using a record gives us value equality and immutability for free.
/// </summary>
public record ProjectFormResult(
    string Name,
    string? ColorHex,
    int? TotalRows,
    string? Notes,
    string? NeedleOrHookSize,
    IReadOnlyList<string> Tags,
    string? ImagePath,
    // Replaces PatternFilePath + PatternFileName — supports multiple files of any type
    IReadOnlyList<PendingProjectFile> ProjectFiles
);
