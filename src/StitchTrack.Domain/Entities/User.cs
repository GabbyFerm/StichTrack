namespace StitchTrack.Domain.Entities;

/// <summary>
/// Represents a registered user with authentication credentials.
/// </summary>
public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string? DisplayName { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public ICollection<Project> Projects { get; private set; } = new List<Project>();

    private User() { }

    // Creates a new user account with validated credentials.
    public static User CreateUser(string email, string passwordHash, string? displayName = null)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));
        }

        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.Trim().ToUpperInvariant(),
            PasswordHash = passwordHash,
            DisplayName = displayName?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDisplayName(string? displayName)
    {
        DisplayName = displayName?.Trim();
    }
}
