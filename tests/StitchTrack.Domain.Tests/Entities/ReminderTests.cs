using FluentAssertions;
using NUnit.Framework;
using StitchTrack.Domain.Entities;

namespace StitchTrack.Domain.Tests.Entities;

[TestFixture]
public class ReminderTests
{
    [Test]
    public void CreateReminder_ShouldCreateEnabledReminder()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var intervalMinutes = 30;

        // Act
        var reminder = Reminder.CreateReminder(projectId, intervalMinutes);

        // Assert
        reminder.Id.Should().NotBe(Guid.Empty);
        reminder.ProjectId.Should().Be(projectId);
        reminder.IntervalMinutes.Should().Be(intervalMinutes);
        reminder.IsEnabled.Should().Be(true);
        reminder.LastTriggeredAt.Should().BeNull();
    }

    [Test]
    public void CreateReminder_WithNegativeInterval_ShouldThrowException()
    {
        // Arrange & Act
        Action act = () => Reminder.CreateReminder(Guid.NewGuid(), -5);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*must be positive*");
    }

    [Test]
    public void CreateReminder_WithZeroInterval_ShouldThrowException()
    {
        // Arrange & Act
        Action act = () => Reminder.CreateReminder(Guid.NewGuid(), 0);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*must be positive*");
    }

    [Test]
    public void Enable_ShouldSetEnabledToTrue()
    {
        // Arrange
        var reminder = Reminder.CreateReminder(Guid.NewGuid(), 30);
        reminder.DisableReminder();

        // Act
        reminder.EnableReminder();

        // Assert
        reminder.IsEnabled.Should().BeTrue();
    }

    [Test]
    public void Disable_ShouldSetEnableToFalse()
    {
        // Arrange
        var reminder = Reminder.CreateReminder(Guid.NewGuid(), 30);

        // Act
        reminder.DisableReminder();

        // Assert
        reminder.IsEnabled.Should().BeFalse();
    }

    [Test]
    public void UpdateInterval_ShouldChangeInterval()
    {
        // Arrange
        var reminder = Reminder.CreateReminder(Guid.NewGuid(), 30);

        // Act
        reminder.UpdateInterval(45);

        // Assert
        reminder.IntervalMinutes.Should().Be(45);
    }

    [Test]
    public void UpdateInterval_WithInvalidValue_ShouldThrowException()
    {
        // Arrange
        var reminder = Reminder.CreateReminder(Guid.NewGuid(), 30);

        // Act
        Action act = () => reminder.UpdateInterval(-10);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void ShouldTrigger_WhenNeverTriggered_ShouldReturnTrue()
    {
        // Arrange
        var reminder = Reminder.CreateReminder(Guid.NewGuid(), 30);

        // Act & Assert
        reminder.ShouldTrigger.Should().BeTrue();
    }

    [Test]
    public void ShouldTrigger_WhenDisabled_ShouldReturnFalse()
    {
        // Arrange
        var reminder = Reminder.CreateReminder(Guid.NewGuid(), 30);
        reminder.DisableReminder();

        // Act & Assert
        reminder.ShouldTrigger.Should().BeFalse();
    }

    [Test]
    public void ShouldTrigger_AfterMarkedTriggered_ShouldReturnFalse()
    {
        // Arrange
        var reminder = Reminder.CreateReminder(Guid.NewGuid(), 30);
        reminder.MarkTriggered();

        // Act & Assert
        reminder.ShouldTrigger.Should().BeFalse();
    }

    [Test]
    public void MarkTriggered_ShouldSetLastTriggeredAt()
    {
        // Arrange
        var reminder = Reminder.CreateReminder(Guid.NewGuid(), 30);

        // Act
        reminder.MarkTriggered();

        // Assert
        reminder.LastTriggeredAt.Should().NotBeNull();
        reminder.LastTriggeredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

}
