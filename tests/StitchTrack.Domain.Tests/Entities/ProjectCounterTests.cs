using FluentAssertions;
using NUnit.Framework;
using StitchTrack.Domain.Entities;

namespace StitchTrack.Domain.Tests.Entities;

[TestFixture]
internal sealed class ProjectCounterTests
{
    private static ProjectCounter CreateCounter(string name = "Rows") =>
        ProjectCounter.Create(Guid.NewGuid(), name, sortOrder: 0);

    // ─── Create ──────────────────────────────────────────────────

    [Test]
    public void Create_ShouldGenerateNewGuid()
    {
        var counter = CreateCounter();

        counter.Id.Should().NotBe(Guid.Empty);
        counter.Name.Should().Be("Rows");
        counter.CurrentCount.Should().Be(0);
        counter.SortOrder.Should().Be(0);
    }

    [Test]
    public void Create_ShouldThrowException_WhenNameIsEmpty()
    {
        Action act = () => ProjectCounter.Create(Guid.NewGuid(), "", sortOrder: 0);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*name*");
    }

    // ─── Increment ───────────────────────────────────────────────

    [Test]
    public void Increment_ShouldIncreaseByOne()
    {
        var counter = CreateCounter();

        counter.Increment();

        counter.CurrentCount.Should().Be(1);
    }

    [TestCase(1)]
    [TestCase(3)]
    [TestCase(10)]
    public void Increment_MultipleTimes_ShouldWork(int times)
    {
        var counter = CreateCounter();

        for (int i = 0; i < times; i++)
            counter.Increment();

        counter.CurrentCount.Should().Be(times);
    }

    // ─── Decrement ───────────────────────────────────────────────

    [Test]
    public void Decrement_WhenAtZero_ShouldStayAtZero()
    {
        var counter = CreateCounter();

        counter.Decrement();

        // CRITICAL BUSINESS RULE — counter never goes negative
        counter.CurrentCount.Should().Be(0);
    }

    [Test]
    public void Decrement_ShouldDecreaseByOne()
    {
        var counter = CreateCounter();
        counter.Increment();
        counter.Increment(); // now at 2

        counter.Decrement();

        counter.CurrentCount.Should().Be(1);
    }

    // ─── Reset ───────────────────────────────────────────────────

    [Test]
    public void Reset_ShouldSetCountToZero()
    {
        var counter = CreateCounter();
        counter.Increment();
        counter.Increment();
        counter.Increment(); // now at 3

        counter.Reset();

        counter.CurrentCount.Should().Be(0);
    }

    // ─── Undo ────────────────────────────────────────────────────

    [Test]
    public void Undo_ShouldRevertLastIncrement()
    {
        var counter = CreateCounter();
        counter.Increment(); // 0 → 1
        counter.Increment(); // 1 → 2

        var undone = counter.UndoLastChange();

        undone.Should().BeTrue();
        counter.CurrentCount.Should().Be(1);
    }

    [Test]
    public void Undo_ShouldRevertLastDecrement()
    {
        var counter = CreateCounter();
        counter.Increment(); // 0 → 1
        counter.Increment(); // 1 → 2
        counter.Decrement(); // 2 → 1

        counter.UndoLastChange();

        counter.CurrentCount.Should().Be(2);
    }

    [Test]
    public void Undo_ShouldReturnFalse_WhenNoHistory()
    {
        var counter = CreateCounter();

        var undone = counter.UndoLastChange();

        undone.Should().BeFalse();
        counter.CurrentCount.Should().Be(0);
    }

    [Test]
    public void Undo_ShouldRemoveHistoryEntry_AfterUndo()
    {
        var counter = CreateCounter();
        counter.Increment();

        counter.UndoLastChange();

        // History should be empty — undo consumed the entry
        counter.CounterHistoryEntries.Should().BeEmpty();
    }

    // ─── Rename ──────────────────────────────────────────────────

    [Test]
    public void Rename_ShouldUpdateName()
    {
        var counter = CreateCounter("Rows");

        counter.Rename("Main body");

        counter.Name.Should().Be("Main body");
    }

    [Test]
    public void Rename_ShouldThrow_WhenNameIsEmpty()
    {
        var counter = CreateCounter();

        Action act = () => counter.Rename("");

        act.Should().Throw<ArgumentException>()
           .WithMessage("*name*");
    }

    // ─── History ─────────────────────────────────────────────────

    [Test]
    public void Increment_ShouldAddHistoryEntry()
    {
        var counter = CreateCounter();

        counter.Increment();

        counter.CounterHistoryEntries.Should().HaveCount(1);
        counter.CounterHistoryEntries.First().OldValue.Should().Be(0);
        counter.CounterHistoryEntries.First().NewValue.Should().Be(1);
    }

    [Test]
    public void MultipleIncrements_ShouldBuildHistoryStack()
    {
        var counter = CreateCounter();

        counter.Increment();
        counter.Increment();
        counter.Increment();

        counter.CounterHistoryEntries.Should().HaveCount(3);
    }
}
