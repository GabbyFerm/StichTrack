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

    /// <summary>
    /// Creates a new user account with validated credentials.
    /// Email is stored in uppercase; password should already be hashed by the caller.
    /// </summary>
    /// <param name="email">User email (required, converted to uppercase)</param>
    /// <param name="passwordHash">Pre-hashed password (required)</param>
    /// <param name="displayName">Optional display name (trimmed)</param>
    /// <returns>A new User instance ready for persistence</returns>
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
