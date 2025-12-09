namespace StitchTrack.Domain.Entities;

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

    public static AppSettings CreateDefault()
    {
        return new AppSettings
        {
            IsFirstRun = true,
            Theme = "Auto",
            HapticFeedbackEnabled = true
        };
    }

    public void CompleteFirstRun()
    {
        IsFirstRun = false;
        FirstRunCompletedAt = DateTime.UtcNow;
    }

    public void EnableSync(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            throw new ArgumentException("Provider cannot be empty", nameof(provider));
        }
        SyncEnabled = true;
        SyncProvider = provider;
    }

    public void DisableSync()
    {
        SyncEnabled = false;
        SyncProvider = null;
    }

    public void UpdateTheme(string theme)
    {
        if (theme != "Light" && theme != "Dark" && theme != "Auto")
        {
            throw new ArgumentException("Invalid theme", nameof(theme));
        }
        Theme = theme;
    }

    public void IncrementProjectCreationCount()
    {
        ProjectCreationCount++;
    }
}
