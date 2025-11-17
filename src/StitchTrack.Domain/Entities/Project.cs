namespace StitchTrack.Domain.Entities;

public class Project
{
    public Guid Id { get; private set; }
    public Guid? UserId { get; set; }
    public required string Name { get; set; }
    public int CurrentCount { get; private set; }
    public int? TotalRows { get; set; }
    public int? RowsPerRepeat { get; set; }
    public string? ColorHex { get; set; }
    public string? Notes { get; set; } // Maps to NVARCHAR(max) in DB
    public bool IsArchived { get; set; }
    public string? ImagePath { get; set; }
    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Private constructor prevents direct instantiation. Forces use of factory method which ensures valid state
    private Project() { }

    public static Project Create(string name, Guid? userId = null)
    {
        // Validate: name is required
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Project name cannot be empty", nameof(name));
        }

        var now = DateTime.UtcNow;

        return new Project
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            CurrentCount = 0,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void IncrementCount()
    {
        CurrentCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DecrementCount()
    {
        if (CurrentCount > 0)
        {
            CurrentCount--;
            UpdatedAt = DateTime.UtcNow;
        }
        // If already at 0, silently do nothing (user-friendly behavior)
    }

    public void ResetCount()
    {
        CurrentCount = 0;
        UpdatedAt = DateTime.UtcNow;
    }
}
