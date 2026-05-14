using FluentAssertions;
using NUnit.Framework;
using StitchTrack.Domain.Entities;

namespace StitchTrack.Domain.Tests.Entities;

[TestFixture]
internal sealed class ProjectTests
{
    [Test]
    public void Create_ShouldGenerateNewGuid_ForNewProject()
    {
        // Arrange & Act
        var project = Project.CreateProject("Test Socks");

        // Assert
        project.Id.Should().NotBe(Guid.Empty);
        project.UserId.Should().BeNull(); // Guest mode = no user
        project.Name.Should().Be("Test Socks");
        project.CurrentCount.Should().Be(0); // Always starts at 0
    }

    [Test]
    public void Create_ShouldThrowException_WhenNameIsEmpty()
    {
        // Arrange & Act
        Action act = () => Project.CreateProject("");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*name*"); // Verify error mentions 'name'
    }

    [Test]
    public void IncrementCount_ShouldIncreaseByOne()
    {
        // Arrange
        var project = Project.CreateProject("Scarf");

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
        var project = Project.CreateProject("Blanket");

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
        var project = Project.CreateProject("Hat");

        // Act
        project.DecrementCount();

        // Assert
        project.CurrentCount.Should().Be(0);
    }

    [Test]
    public void DecrementCount_WhenAtZero_ShouldStayAtZero()
    {
        // Arrange
        var project = Project.CreateProject("Mittens");
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
        var project = Project.CreateProject("Sweater");
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
        var project = Project.CreateProject("Socks");
        var originalTimestamp = project.UpdatedAt;

        // Wait a tiny bit to ensure time difference
        Thread.Sleep(10);

        // Act
        project.IncrementCount();

        // Assert
        project.UpdatedAt.Should().BeAfter(originalTimestamp);
    }

    // ─── UpdateProjectDetails ────────────────────────────────────────────

    [Test]
    public void UpdateProjectDetails_ShouldSetNeedleOrHookSize()
    {
        // Arrange
        var project = Project.CreateProject("Socks");

        // Act
        project.UpdateProjectDetails(needleOrHookSize: "5.0mm");

        // Assert
        project.NeedleOrHookSize.Should().Be("5.0mm");
    }

    [Test]
    public void UpdateProjectDetails_ShouldClearNeedleOrHookSize_WhenNull()
    {
        // Arrange
        var project = Project.CreateProject("Socks");
        project.UpdateProjectDetails(needleOrHookSize: "5.0mm");

        // Act
        project.UpdateProjectDetails(needleOrHookSize: null);

        // Assert
        project.NeedleOrHookSize.Should().BeNull();
    }

    [Test]
    public void UpdateProjectDetails_ShouldUpdateTimestamp()
    {
        // Arrange
        var project = Project.CreateProject("Socks");
        var original = project.UpdatedAt;
        Thread.Sleep(10);

        // Act
        project.UpdateProjectDetails(notes: "Some notes");

        // Assert
        project.UpdatedAt.Should().BeAfter(original);
    }

    [Test]
    public void AddTag_ShouldAddTagToCollection()
    {
        // Arrange
        var project = Project.CreateProject("Hat");

        // Act
        project.AddTag("Knitting", colorIndex: 0);

        // Assert
        project.Tags.Should().HaveCount(1);
        project.Tags.First().Name.Should().Be("Knitting");
    }

    [Test]
    public void AddTag_ShouldIgnoreDuplicate_CaseInsensitive()
    {
        // Arrange
        var project = Project.CreateProject("Hat");
        project.AddTag("Knitting", colorIndex: 0);

        // Act — same name, different casing
        project.AddTag("knitting", colorIndex: 1);

        // Assert — still only one tag
        project.Tags.Should().HaveCount(1);
    }

    [Test]
    public void AddTag_ShouldSupportMultipleDifferentTags()
    {
        // Arrange
        var project = Project.CreateProject("Blanket");

        // Act
        project.AddTag("Knitting", colorIndex: 0);
        project.AddTag("Amigurumi", colorIndex: 1);
        project.AddTag("Clothing", colorIndex: 2);

        // Assert
        project.Tags.Should().HaveCount(3);
    }

    [Test]
    public void RemoveTag_ShouldRemoveTagFromCollection()
    {
        // Arrange
        var project = Project.CreateProject("Hat");
        project.AddTag("Knitting", colorIndex: 0);

        // Act
        project.RemoveTag("Knitting");

        // Assert
        project.Tags.Should().BeEmpty();
    }

    [Test]
    public void RemoveTag_ShouldBeNoOp_WhenTagNotFound()
    {
        // Arrange
        var project = Project.CreateProject("Hat");
        project.AddTag("Knitting", colorIndex: 0);

        // Act — removing a tag that doesn't exist should not throw
        Action act = () => project.RemoveTag("Crochet");

        // Assert
        act.Should().NotThrow();
        project.Tags.Should().HaveCount(1); // original tag still there
    }

    [Test]
    public void ClearTags_ShouldRemoveAllTags()
    {
        // Arrange
        var project = Project.CreateProject("Blanket");
        project.AddTag("Knitting", colorIndex: 0);
        project.AddTag("Amigurumi", colorIndex: 1);

        // Act
        project.ClearTags();

        // Assert
        project.Tags.Should().BeEmpty();
    }
}
