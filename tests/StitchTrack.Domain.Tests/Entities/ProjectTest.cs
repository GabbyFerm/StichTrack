using FluentAssertions;
using NUnit.Framework;
using StitchTrack.Domain.Entities;

namespace StitchTrack.Domain.Tests.Entities;

[TestFixture]
internal sealed class ProjectTests
{
    // ─── Create ──────────────────────────────────────────────────

    [Test]
    public void Create_ShouldGenerateNewGuid_ForNewProject()
    {
        var project = Project.CreateProject("Test Socks");

        project.Id.Should().NotBe(Guid.Empty);
        project.UserId.Should().BeNull();
        project.Name.Should().Be("Test Socks");
        project.CurrentCount.Should().Be(0);
    }

    [Test]
    public void Create_ShouldThrowException_WhenNameIsEmpty()
    {
        Action act = () => Project.CreateProject("");

        act.Should().Throw<ArgumentException>()
           .WithMessage("*name*");
    }

    // ─── UpdateProjectDetails ────────────────────────────────────

    [Test]
    public void UpdateProjectDetails_ShouldSetNeedleOrHookSize()
    {
        var project = Project.CreateProject("Socks");

        project.UpdateProjectDetails(needleOrHookSize: "5.0mm");

        project.NeedleOrHookSize.Should().Be("5.0mm");
    }

    [Test]
    public void UpdateProjectDetails_ShouldClearNeedleOrHookSize_WhenNull()
    {
        var project = Project.CreateProject("Socks");
        project.UpdateProjectDetails(needleOrHookSize: "5.0mm");

        project.UpdateProjectDetails(needleOrHookSize: null);

        project.NeedleOrHookSize.Should().BeNull();
    }

    [Test]
    public void UpdateProjectDetails_ShouldUpdateTimestamp()
    {
        var project = Project.CreateProject("Socks");
        var original = project.UpdatedAt;
        Thread.Sleep(10);

        project.UpdateProjectDetails(notes: "Some notes");

        project.UpdatedAt.Should().BeAfter(original);
    }

    // ─── Counters ────────────────────────────────────────────────

    [Test]
    public void AddCounter_ShouldAddCounterToCollection()
    {
        var project = Project.CreateProject("Sweater");

        project.AddCounter("Rows", sortOrder: 0);

        project.Counters.Should().HaveCount(1);
        project.Counters.First().Name.Should().Be("Rows");
        project.Counters.First().SortOrder.Should().Be(0);
    }

    [Test]
    public void AddCounter_ShouldSupportMultipleCounters()
    {
        var project = Project.CreateProject("Sweater");

        project.AddCounter("Rows", sortOrder: 0);
        project.AddCounter("Stitches", sortOrder: 1);

        project.Counters.Should().HaveCount(2);
    }

    [Test]
    public void AddCounter_ShouldThrow_WhenNameIsEmpty()
    {
        var project = Project.CreateProject("Sweater");

        Action act = () => project.AddCounter("", sortOrder: 0);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*name*");
    }

    [Test]
    public void RemoveCounter_ShouldRemoveCounterFromCollection()
    {
        var project = Project.CreateProject("Sweater");
        var counter = project.AddCounter("Rows", sortOrder: 0);

        project.RemoveCounter(counter.Id);

        project.Counters.Should().BeEmpty();
    }

    [Test]
    public void RemoveCounter_ShouldBeNoOp_WhenCounterNotFound()
    {
        var project = Project.CreateProject("Sweater");
        project.AddCounter("Rows", sortOrder: 0);

        Action act = () => project.RemoveCounter(Guid.NewGuid());

        act.Should().NotThrow();
        project.Counters.Should().HaveCount(1);
    }

    // ─── Tags ────────────────────────────────────────────────────

    [Test]
    public void AddTag_ShouldAddTagToCollection()
    {
        var project = Project.CreateProject("Hat");

        project.AddTag("Knitting", colorIndex: 0);

        project.Tags.Should().HaveCount(1);
        project.Tags.First().Name.Should().Be("Knitting");
    }

    [Test]
    public void AddTag_ShouldIgnoreDuplicate_CaseInsensitive()
    {
        var project = Project.CreateProject("Hat");
        project.AddTag("Knitting", colorIndex: 0);

        project.AddTag("knitting", colorIndex: 1);

        project.Tags.Should().HaveCount(1);
    }

    [Test]
    public void AddTag_ShouldSupportMultipleDifferentTags()
    {
        var project = Project.CreateProject("Blanket");

        project.AddTag("Knitting", colorIndex: 0);
        project.AddTag("Amigurumi", colorIndex: 1);
        project.AddTag("Clothing", colorIndex: 2);

        project.Tags.Should().HaveCount(3);
    }

    [Test]
    public void RemoveTag_ShouldRemoveTagFromCollection()
    {
        var project = Project.CreateProject("Hat");
        project.AddTag("Knitting", colorIndex: 0);

        project.RemoveTag("Knitting");

        project.Tags.Should().BeEmpty();
    }

    [Test]
    public void RemoveTag_ShouldBeNoOp_WhenTagNotFound()
    {
        var project = Project.CreateProject("Hat");
        project.AddTag("Knitting", colorIndex: 0);

        Action act = () => project.RemoveTag("Crochet");

        act.Should().NotThrow();
        project.Tags.Should().HaveCount(1);
    }

    [Test]
    public void ClearTags_ShouldRemoveAllTags()
    {
        var project = Project.CreateProject("Blanket");
        project.AddTag("Knitting", colorIndex: 0);
        project.AddTag("Amigurumi", colorIndex: 1);

        project.ClearTags();

        project.Tags.Should().BeEmpty();
    }
}
