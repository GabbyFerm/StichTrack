using FluentAssertions;
using StitchTrack.Domain.Entities;

namespace StitchTrack.Domain.Tests.Entities;

[TestFixture]
public class ProjectTests
{
    [Test]
    public void Create_ShouldGenerateNewGuid_ForGuestProject()
    {
        // Arrange & Act
        var project = Project.Create("Test Socks");

        // Assert
        project.Id.Should().NotBe(Guid.Empty);
        project.UserId.Should().BeNull(); // Guest mode = no user
        project.Name.Should().Be("Test Socks");
        project.CurrentCount.Should().Be(0); // Always starts at 0
    }

    [Test]
    public void Create_ShouldThowException_WhenNameIsEmpty()
    {
        // Arrange & Act
        Action act = () => Project.Create("");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*name*"); // Verify error mentions 'name'
    }

    [Test]
    public void IncrementCount_ShouldIncreaseByOne()
    {
        // Arrange
        var project = Project.Create("Scarf");

        // Act
        project.IncrementCount();

        // Assert
        project.CurrentCount.Should().Be(1);
    }

    [TestCase(1)]
    [TestCase(3)]
    [TestCase(10)]
    public void IncrementCount_MultipleTimesShouldWork(int times)
    {
        // Arrange
        var project = Project.Create("Blanket");

        // Act
        for (int i = 0; i < times; i++)
        {
            project.IncrementCount();
        }

        // Assert
        project.CurrentCount.Should().Be(times);
    }

    [Test]
    public void DecrementCount_ShouldDecreaseByOne()
    {
        // Arrange
        var project = Project.Create("Hat");

        // Act
        project.DecrementCount();

        // Assert
        project.CurrentCount.Should().Be(0);
    }

    [Test]
    public void DecrementCount_WhenAtZero_ShouldStayAtZero()
    {
        // Arrange
        var project = Project.Create("Mittens");
        // CurrentCount is already 0

        // Act
        project.DecrementCount();

        // Assert - CRITICAL BUSINESS RULE
        project.CurrentCount.Should().Be(0);
    }

    [Test]
    public void ResetCount_ShouldSetToZero()
    {
        // Arrange
        var project = Project.Create("Sweater");
        project.IncrementCount();
        project.IncrementCount();
        project.IncrementCount(); // Now at 3

        // Act
        project.ResetCount();

        // Assert
        project.CurrentCount.Should().Be(0);
    }

    [Test]
    public void IncrementCount_ShouldUpdateTimestamp()
    {
        // Arrange
        var project = Project.Create("Socks");
        var originalTimestamp = project.UpdatedAt;

        // Wait a tiny bit to ensure time difference
        Thread.Sleep(10);

        // Act
        project.IncrementCount();

        // Assert
        project.UpdatedAt.Should().BeAfter(originalTimestamp);
    }
}
