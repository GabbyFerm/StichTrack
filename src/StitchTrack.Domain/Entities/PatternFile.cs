namespace StitchTrack.Domain.Entities;

/// <summary>
/// Represents an uploaded pattern file (PDF, image) attached to a project.
/// </summary>
public class PatternFile
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    public string FileName { get; private set; } = string.Empty;
    public string? FilePath { get; private set; }
    public string? FileUrl { get; private set; }
    public DateTime UploadedAt { get; private set; }

    public long FileSizeBytes { get; private set; }
    public string? ContentType { get; private set; }

    private PatternFile() { }

    public static PatternFile CreatePatternFile(Guid projectId, string fileName, string? filePath, long fileSizeBytes, string? contentType = null)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name cannot be empty", nameof(fileName));
        }

        if (fileSizeBytes < 0)
        {
            throw new ArgumentException("File size cannot be negative", nameof(fileSizeBytes));
        }

        return new PatternFile
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            FileName = fileName.Trim(),
            FilePath = filePath,
            FileUrl = null,
            FileSizeBytes = fileSizeBytes,
            ContentType = contentType,
            UploadedAt = DateTime.UtcNow
        };
    }

    public void SetCloudUrl(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            throw new ArgumentException("File URL cannot be empty", nameof(fileUrl));
        }

        FileUrl = fileUrl.Trim();
    }
}
