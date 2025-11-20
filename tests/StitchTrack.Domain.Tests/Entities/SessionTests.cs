using FluentAssertions;
using StitchTrack.Domain.Entities;

namespace StitchTrack.Domain.Tests.Entities;

[TestFixture]

public class SessionTests
{
    [Test]
    public void Start_ShouldCreateNewSession()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var startingRowCount = 5;

        // Act
        var session = Session.StartSession(projectId, startingRowCount);

        // Assert
        session.Id.Should().NotBe(Guid.Empty);
        session.ProjectId.Should().Be(projectId);
        session.StartingRowCount.Should().Be(startingRowCount);
        session.EndedAt.Should().BeNull();
        session.DurationSeconds.Should().Be(0);
        session.IsActive.Should().BeTrue();
    }

    [Test]
    public void End_ShouldCalculateDurationCorrectly()
    {
        // Arrange
        var session = Session.StartSession(Guid.NewGuid(), 10);

        // Act
        session.EndSession(15);

        // Assert
        // Duration should be calculated, even if very small (0-1 seconds)
        session.DurationSeconds.Should().BeGreaterThanOrEqualTo(0);
        session.EndedAt.Should().NotBeNull();

        // Calculate expected duration manually
        var expectedDuration = (int)(session.EndedAt!.Value - session.StartedAt).TotalSeconds;
        session.DurationSeconds.Should().Be(expectedDuration);
    }

    [Test]
    public void End_WhenAlreadyEnded_ShouldThrowException()
    {
        // Arrange
        var session = Session.StartSession(Guid.NewGuid());
        session.EndSession(5);

        // Act
        Action act = () => session.EndSession(10);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already ended*");
    }

    [Test]
    public void RowsCompleted_ShouldCalculateCorrectly()
    {
        // Arrange
        var session = Session.StartSession(Guid.NewGuid(), 10);
        session.EndSession(25);

        // Act
        var rowsCompleted = session.RowsCompleted;

        // Assert
        rowsCompleted.Should().Be(15);
    }

    [Test]
    public void RowsCompleted_WhenNoStartingCount_ShouldBeNull()
    {
        // Arrange
        var session = Session.StartSession(Guid.NewGuid());
        session.EndSession();

        // Act
        var rowsCompleted = session.RowsCompleted;

        // Assert
        rowsCompleted.Should().BeNull();
    }

    [Test]
    public void End_WithoutRowCounts_ShouldWork()
    {
        // Arrange
        var session = Session.StartSession(Guid.NewGuid());

        // Act
        session.EndSession();

        // Assert
        session.EndedAt.Should().NotBeNull();
        session.IsActive.Should().BeFalse();
        session.StartingRowCount.Should().BeNull();
        session.EndingRowCount.Should().BeNull();
    }

    [Test]
    public void IsActive_WhenNotEnded_ShouldBeTrue()
    {
        // Arrange
        var session = Session.StartSession(Guid.NewGuid());

        // Act & Assert
        session.IsActive.Should().BeTrue();
    }

    [Test]
    public void IsActive_WhenEnded_ShouldBeFalse()
    {
        // Arrange
        var session = Session.StartSession(Guid.NewGuid());

        // Act
        session.EndSession();

        // Assert
        session.IsActive.Should().BeFalse();
    }
}
