// Copyright (c) 2026 Gabriella Frank Ferm / Frank Ferm Design. All rights reserved.
namespace StitchTrack.Domain.Entities;

/// <summary>
/// A file attached to a project — either a pattern file (PDF/image)
/// or an inspiration photo. Replaces PatternFile to support both types
/// and multiple files per project.
/// </summary>
public class ProjectFile
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    public ProjectFileType FileType { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string? FilePath { get; private set; }   // local path
    public string? FileUrl { get; private set; }    // cloud url (Phase 3)
    public long FileSizeBytes { get; private set; }
    public string? ContentType { get; private set; }
    public DateTime UploadedAt { get; private set; }

    private ProjectFile() { }

    public static ProjectFile Create(
        Guid projectId,
        string fileName,
        string? filePath,
        long fileSizeBytes,
        ProjectFileType fileType,
        string? contentType = null)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty", nameof(fileName));

        if (fileSizeBytes < 0)
            throw new ArgumentException("File size cannot be negative", nameof(fileSizeBytes));

        return new ProjectFile
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            FileName = fileName.Trim(),
            FilePath = filePath,
            FileUrl = null,
            FileSizeBytes = fileSizeBytes,
            ContentType = contentType,
            FileType = fileType,
            UploadedAt = DateTime.UtcNow
        };
    }

    // Used when syncing to cloud (Phase 3)
    public void SetCloudUrl(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
            throw new ArgumentException("File URL cannot be empty", nameof(fileUrl));

        FileUrl = fileUrl.Trim();
    }
}
