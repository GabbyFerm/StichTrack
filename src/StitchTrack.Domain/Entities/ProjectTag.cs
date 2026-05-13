namespace StitchTrack.Domain.Entities;

/// <summary>
/// A single free-text tag belonging to a project.
/// Tags are owned by the Project aggregate — never created or deleted
/// independently. ColorIndex is the position in TagColors.Palette
/// assigned when the tag is saved, so the chip color is stable.
/// </summary>
public class ProjectTag
{
    public Guid Id { get; private set; }

    // Foreign key back to the owning project
    public Guid ProjectId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    // Index into TagColors.Palette — stored so color is stable across loads
    public int ColorIndex { get; private set; }

    // Required by EF Core — not for direct use
    private ProjectTag() { }

    /// <summary>
    /// Creates a validated tag. ColorIndex should be assigned by the
    /// caller using: tagPosition % TagColors.Palette.Length
    /// </summary>
    public static ProjectTag Create(Guid projectId, string name, int colorIndex)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tag name cannot be empty.", nameof(name));

        return new ProjectTag
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Name = name.Trim(),
            ColorIndex = colorIndex
        };
    }
}
