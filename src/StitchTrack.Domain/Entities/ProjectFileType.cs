namespace StitchTrack.Domain.Entities;

/// <summary>
/// Distinguishes between pattern files and inspiration photos
/// attached to a project. Both are stored in the ProjectFiles table.
/// </summary>
public enum ProjectFileType
{
    Pattern = 0,
    InspirationPhoto = 1
}
