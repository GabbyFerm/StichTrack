// Copyright (c) 2026 Gabriella Frank Ferm / Frank Ferm Design. All rights reserved.
namespace StitchTrack.Domain.Entities;

/// <summary>
/// Planned feature — not yet implemented.
/// Represents a recurring reminder to work on a project.
/// Infrastructure exists (EF config, navigation property on Project)
/// but no repository, ViewModel, or UI has been built.
/// Tracked for Phase 6.
/// </summary>
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

    /// <summary>
    /// Creates a new reminder with the specified interval.
    /// Reminders are enabled by default and have never been triggered.
    /// </summary>
    /// <param name="projectId">The project this reminder belongs to</param>
    /// <param name="intervalMinutes">The interval between triggers in minutes (must be > 0)</param>
    /// <returns>A new Reminder instance ready for persistence</returns>
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

    /// <summary>
    /// Enables the reminder so it can trigger again.
    /// </summary>
    public void EnableReminder()
    {
        IsEnabled = true;
    }

    /// <summary>
    /// Disables the reminder so it will not trigger.
    /// </summary>
    public void DisableReminder()
    {
        IsEnabled = false;
    }

    /// <summary>
    /// Updates the interval between reminder triggers.
    /// </summary>
    /// <param name="intervalMinutes">The new interval in minutes (must be > 0)</param>
    public void UpdateInterval(int intervalMinutes)
    {
        if (intervalMinutes <= 0)
        {
            throw new ArgumentException("Interval must be positive", nameof(intervalMinutes));
        }

        IntervalMinutes = intervalMinutes;
    }

    /// <summary>
    /// Records that the reminder has been triggered now.
    /// Updates LastTriggeredAt to the current time.
    /// </summary>
    public void MarkTriggered()
    {
        LastTriggeredAt = DateTime.UtcNow;
    }
}
