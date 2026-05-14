namespace StitchTrack.Domain.Entities;

/// <summary>
/// A note attached to a specific row number in a project.
/// Used for reminders like "decrease here" or "change color".
/// </summary>
public class RowNote
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    public int RowNumber { get; private set; }
    public string? NoteText { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private RowNote() { }

    /// <summary>
    /// Creates a validated row note with row number and optional text.
    /// </summary>
    /// <param name="projectId">The project this note belongs to</param>
    /// <param name="rowNumber">The row number for this note (must be >= 0)</param>
    /// <param name="noteText">Optional reminder text (trimmed)</param>
    /// <returns>A new RowNote instance ready for persistence</returns>
    public static RowNote CreateRowNote(Guid projectId, int rowNumber, string? noteText)
    {
        if (rowNumber < 0)
        {
            throw new ArgumentException("Row number cannot be negative", nameof(rowNumber));
        }

        return new RowNote
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            RowNumber = rowNumber,
            NoteText = noteText?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateText(string? newText)
    {
        NoteText = newText?.Trim();
    }
}
