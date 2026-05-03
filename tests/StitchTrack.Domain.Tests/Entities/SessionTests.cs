using FluentAssertions;
using NUnit.Framework;
using StitchTrack.Domain.Entities;

namespace StitchTrack.Domain.Tests.Entities;

[TestFixture]
internal sealed class SessionTests
{
    [Test]
    public void StartSession_ShouldInitializeActiveSession()
    {
        // Arrange
        var projectId = Guid.NewGuid();

        // Act
        var session = Session.StartSession(projectId, startingRowCount: 10);

        // Assert
        session.ProjectId.Should().Be(projectId);
        session.StartingRowCount.Should().Be(10);
        session.IsActive.Should().BeTrue();
        session.EndedAt.Should().BeNull();
        session.DurationSeconds.Should().Be(0);
        session.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void EndSession_ShouldCalculateDurationAndRows()
    {
        // Arrange
        var session = Session.StartSession(Guid.NewGuid(), startingRowCount: 10);
        Thread.Sleep(100); // Sleep briefly to ensure time elapses for DurationSeconds

        // Act
        session.EndSession(endingRowCount: 15);

        // Assert
        session.IsActive.Should().BeFalse();
        session.EndedAt.Should().NotBeNull();
        session.EndingRowCount.Should().Be(15);

        // Rows completed = 15 - 10
        session.RowsCompleted.Should().Be(5);
        session.DurationSeconds.Should().BeGreaterThanOrEqualTo(0);
    }

    [Test]
    public void EndSession_WhenAlreadyEnded_ShouldThrowException()
    {
        // Arrange
        var session = Session.StartSession(Guid.NewGuid());
        session.EndSession();

        // Act
        Action act = () => session.EndSession();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already ended*");
    }

    [Test]
    public void RowsCompleted_WithoutRowData_ShouldReturnNull()
    {
        // Arrange & Act
        var session = Session.StartSession(Guid.NewGuid()); // no starting row count
        session.EndSession(); // no ending row count

        // Assert
        session.RowsCompleted.Should().BeNull();
    }
}
