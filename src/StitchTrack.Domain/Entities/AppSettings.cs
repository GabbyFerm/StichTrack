namespace StitchTrack.Domain.Entities;

/// <summary>
/// Application-wide settings (single-row table).
/// Manages onboarding state, theme preference, sync configuration, and haptics.
/// </summary>
public class AppSettings
{
    public Guid Id { get; private set; } = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public bool IsFirstRun { get; private set; } = true;
    public DateTime? FirstRunCompletedAt { get; private set; }

    public bool SyncEnabled { get; private set; }
    public string? SyncProvider { get; private set; }
    public DateTime? LastSuccessfulSync { get; private set; }

    public string Theme { get; private set; } = "Auto";
    public bool HapticFeedbackEnabled { get; private set; } = true;
    public int ProjectCreationCount { get; private set; }

    private AppSettings() { }

    /// <summary>
    /// Creates default AppSettings for first-run scenarios.
    /// Initializes with onboarding enabled, auto theme, and haptics on.
    /// </summary>
    /// <returns>A new AppSettings instance with default values</returns>
    public static AppSettings CreateDefault()
    {
        return new AppSettings
        {
            IsFirstRun = true,
            Theme = "Auto",
            HapticFeedbackEnabled = true
        };
    }

    /// <summary>
    /// Marks the app as no longer in first-run state after onboarding completes.
    /// </summary>
    public void CompleteFirstRun()
    {
        IsFirstRun = false;
        FirstRunCompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Resets first run flag so onboarding shows again on next app launch.
    /// </summary>
    public void ResetFirstRun()
    {
        IsFirstRun = true;
        FirstRunCompletedAt = null;
    }

    /// <summary>
    /// Enables cloud sync and sets the provider (e.g., "iCloud", "GoogleDrive").
    /// </summary>
    /// <param name="provider">The cloud sync provider name (required)</param>
    public void EnableSync(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            throw new ArgumentException("Provider cannot be empty", nameof(provider));
        }
        SyncEnabled = true;
        SyncProvider = provider;
    }

    /// <summary>
    /// Disables cloud sync and clears the provider.
    /// </summary>
    public void DisableSync()
    {
        SyncEnabled = false;
        SyncProvider = null;
    }

    /// <summary>
    /// Updates the app theme preference.
    /// Valid values: "Light", "Dark", "Auto".
    /// </summary>
    /// <param name="theme">The new theme setting</param>
    public void UpdateTheme(string theme)
    {
        if (theme != "Light" && theme != "Dark" && theme != "Auto")
        {
            throw new ArgumentException("Invalid theme", nameof(theme));
        }
        Theme = theme;
    }

    /// <summary>
    /// Flips the haptic feedback toggle and returns the new value.
    /// </summary>
    public bool ToggleHapticFeedback()
    {
        HapticFeedbackEnabled = !HapticFeedbackEnabled;
        return HapticFeedbackEnabled;
    }

    /// <summary>
    /// Increments the project creation counter for analytics or feature gating.
    /// </summary>
    public void IncrementProjectCreationCount()
    {
        ProjectCreationCount++;
    }
}
