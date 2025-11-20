namespace StitchTrack.Domain.Entities;

public class Reminder
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    public int IntervalMinutes { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateTime? LastTriggeredAt { get; private set; }

    public bool ShouldTrigger
    {
        get
        {
            if (!IsEnabled)
            {
                return false;
            }

            if (LastTriggeredAt == null)
            {
                return true;
            }

            var timeSinceLastTrigger = DateTime.UtcNow - LastTriggeredAt.Value;
            return timeSinceLastTrigger.TotalMinutes >= IntervalMinutes;
        }
    }

    private Reminder() { }

    public static Reminder CreateReminder(Guid projectId, int intervalMinutes)
    {
        if (intervalMinutes <= 0)
        {
            throw new ArgumentException("Interval must be positive", nameof(intervalMinutes));
        }

        return new Reminder
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            IntervalMinutes = intervalMinutes,
            IsEnabled = true,
            LastTriggeredAt = null
        };
    }

    public void EnableReminder()
    {
        IsEnabled = true;
    }

    public void DisableReminder()
    {
        IsEnabled = false;
    }

    public void UpdateInterval(int intervalMinutes)
    {
        if (intervalMinutes <= 0)
        {
            throw new ArgumentException("Interval must be positive", nameof(intervalMinutes));
        }

        IntervalMinutes = intervalMinutes;
    }

    public void MarkTriggered()
    {
        LastTriggeredAt = DateTime.UtcNow;
    }
}
