// Copyright (c) 2026 Gabriella Frank Ferm / Frank Ferm Design. All rights reserved.
using FluentAssertions;
using Moq;
using NUnit.Framework;
using StitchTrack.Application.Interfaces;
using StitchTrack.Application.ViewModels;
using StitchTrack.Domain.Entities;
using StitchTrack.Domain.Interfaces;

namespace StitchTrack.Application.Tests.ViewModels;

[TestFixture]
internal sealed class ProjectCounterViewModelTests
{
    private ProjectCounterViewModel _viewModel = null!;
    private Mock<IProjectRepository> _mockProjectRepo = null!;
    private Mock<IProjectCounterRepository> _mockCounterRepo = null!;
    private Mock<IRowNoteRepository> _mockRowNoteRepo = null!;
    private Mock<ISessionRepository> _mockSessionRepo = null!;
    private Mock<IDialogService> _mockDialogService = null!;
    private Mock<INavigationService> _mockNavigationService = null!;
    private Mock<IHapticsService> _mockHapticsService = null!;

    private Project _project = null!;

    [SetUp]
    public void Setup()
    {
        _mockProjectRepo = new Mock<IProjectRepository>();
        _mockCounterRepo = new Mock<IProjectCounterRepository>();
        _mockRowNoteRepo = new Mock<IRowNoteRepository>();
        _mockSessionRepo = new Mock<ISessionRepository>();
        _mockDialogService = new Mock<IDialogService>();
        _mockNavigationService = new Mock<INavigationService>();
        _mockHapticsService = new Mock<IHapticsService>();

        _project = Project.CreateProject("Test Socks");

        _viewModel = CreateViewModel();
        _viewModel.ProjectId = _project.Id;
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private ProjectCounterViewModel CreateViewModel() =>
        new(
            _mockProjectRepo.Object,
            _mockCounterRepo.Object,
            _mockRowNoteRepo.Object,
            _mockSessionRepo.Object,
            _mockDialogService.Object,
            _mockNavigationService.Object,
            _mockHapticsService.Object);

    private ProjectCounter MakeCounter(string name = "Rows", int sortOrder = 0) =>
        ProjectCounter.Create(_project.Id, name, sortOrder);

    private void SetupLoadedProject(IEnumerable<ProjectCounter>? counters = null, IEnumerable<RowNote>? rowNotes = null)
    {
        _mockProjectRepo
            .Setup(x => x.GetByIdWithoutHistoryAsync(_project.Id))
            .ReturnsAsync(_project);

        _mockCounterRepo
            .Setup(x => x.GetByProjectIdAsync(_project.Id))
            .ReturnsAsync(counters ?? [MakeCounter()]);

        _mockRowNoteRepo
            .Setup(x => x.GetByProjectIdAsync(_project.Id))
            .ReturnsAsync(rowNotes ?? []);
    }

    // ─── Constructor ─────────────────────────────────────────────────────────

    [Test]
    public void Constructor_ThrowsArgumentNullException_WhenProjectRepositoryIsNull()
    {
        FluentActions
            .Invoking(() => new ProjectCounterViewModel(
                null!,
                _mockCounterRepo.Object,
                _mockRowNoteRepo.Object,
                _mockSessionRepo.Object,
                _mockDialogService.Object,
                _mockNavigationService.Object,
                _mockHapticsService.Object))
            .Should().Throw<ArgumentNullException>()
            .WithParameterName("projectRepository");
    }

    [Test]
    public void Constructor_ThrowsArgumentNullException_WhenCounterRepositoryIsNull()
    {
        FluentActions
        .Invoking(() => new ProjectCounterViewModel(
                _mockProjectRepo.Object,
                null!,
                _mockRowNoteRepo.Object,
                _mockSessionRepo.Object,
                _mockDialogService.Object,
            _mockNavigationService.Object,
            _mockHapticsService.Object))
            .Should().Throw<ArgumentNullException>()
            .WithParameterName("counterRepository");
    }

    [Test]
    public void Constructor_ThrowsArgumentNullException_WhenDialogServiceIsNull()
    {
        FluentActions
            .Invoking(() => new ProjectCounterViewModel(
                _mockProjectRepo.Object,
                _mockCounterRepo.Object,
                _mockRowNoteRepo.Object,
                _mockSessionRepo.Object,
                null!,
                _mockNavigationService.Object,
                _mockHapticsService.Object))
            .Should().Throw<ArgumentNullException>()
            .WithParameterName("dialogService");
    }

    [Test]
    public void Constructor_ShouldInitialiseWithSessionNotRunning()
    {
        _viewModel.IsSessionRunning.Should().BeFalse();
    }

    [Test]
    public void Constructor_ShouldInitialiseWithZeroCurrentCount()
    {
        _viewModel.CurrentCount.Should().Be(0);
    }

    [Test]
    public void Constructor_ShouldInitialiseWithEmptyCounterList()
    {
        _viewModel.Counters.Should().BeEmpty();
    }

    // ─── LoadProjectAsync ────────────────────────────────────────────────────

    [Test]
    public async Task LoadProjectAsync_ShouldPopulateCounters()
    {
        var counter = MakeCounter("Rows", 0);
        SetupLoadedProject(counters: [counter]);

        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        _viewModel.Counters.Should().HaveCount(1);
        _viewModel.Counters[0].Name.Should().Be("Rows");
    }

    [Test]
    public async Task LoadProjectAsync_ShouldFireCountersChanged()
    {
        SetupLoadedProject();
        var fired = false;
        _viewModel.CountersChanged += (_, _) => fired = true;

        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        fired.Should().BeTrue();
    }

    [Test]
    public async Task LoadProjectAsync_ShouldFireRowNotesChanged()
    {
        SetupLoadedProject();
        var fired = false;
        _viewModel.RowNotesChanged += (_, _) => fired = true;

        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        fired.Should().BeTrue();
    }

    [Test]
    public async Task LoadProjectAsync_WhenProjectNotFound_ShouldShowAlert()
    {
        _mockProjectRepo
            .Setup(x => x.GetByIdWithoutHistoryAsync(_project.Id))
            .ReturnsAsync((Project?)null);

        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        _mockDialogService.Verify(
            x => x.ShowAlertAsync("Error", It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);
    }

    [Test]
    public async Task LoadProjectAsync_ShouldExposeProjectName()
    {
        SetupLoadedProject();

        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        _viewModel.ProjectName.Should().Be("Test Socks");
    }

    // ─── IncrementCounter ────────────────────────────────────────────────────

    [Test]
    public async Task IncrementCounter_ShouldIncreaseCount()
    {
        var counter = MakeCounter();
        SetupLoadedProject(counters: [counter]);
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        _viewModel.IncrementCounter(counter.Id);
        await Task.Delay(50).ConfigureAwait(false);

        counter.CurrentCount.Should().Be(1);
    }

    [Test]
    public async Task IncrementCounter_ShouldFireCountersChanged()
    {
        var counter = MakeCounter();
        SetupLoadedProject(counters: [counter]);
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        var fired = false;
        _viewModel.CountersChanged += (_, _) => fired = true;

        _viewModel.IncrementCounter(counter.Id);

        fired.Should().BeTrue();
    }

    [Test]
    public async Task IncrementCounter_ShouldTriggerHaptics()
    {
        var counter = MakeCounter();
        SetupLoadedProject(counters: [counter]);
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        _viewModel.IncrementCounter(counter.Id);

        _mockHapticsService.Verify(x => x.Click(), Times.Once);
    }

    [Test]
    public async Task IncrementCounter_OnPrimaryCounter_ShouldUpdateCurrentCount()
    {
        var counter = MakeCounter("Rows", sortOrder: 0);
        SetupLoadedProject(counters: [counter]);
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        _viewModel.IncrementCounter(counter.Id);
        await Task.Delay(50).ConfigureAwait(false);

        _viewModel.CurrentCount.Should().Be(1);
    }

    [Test]
    public async Task IncrementCounter_OnSecondaryCounter_ShouldNotUpdateCurrentCount()
    {
        var primary = MakeCounter("Rows", sortOrder: 0);
        var secondary = MakeCounter("Stitches", sortOrder: 1);
        SetupLoadedProject(counters: [primary, secondary]);
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        _viewModel.IncrementCounter(secondary.Id);
        await Task.Delay(50).ConfigureAwait(false);

        _viewModel.CurrentCount.Should().Be(0);
    }

    [Test]
    public async Task IncrementCounter_WithUnknownId_ShouldDoNothing()
    {
        SetupLoadedProject();
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        Action act = () => _viewModel.IncrementCounter(Guid.NewGuid());

        act.Should().NotThrow();
        _mockHapticsService.Verify(x => x.Click(), Times.Never);
    }

    // ─── DecrementCounter ────────────────────────────────────────────────────

    [Test]
    public async Task DecrementCounter_ShouldDecreaseCount()
    {
        var counter = MakeCounter();
        SetupLoadedProject(counters: [counter]);
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);
        _viewModel.IncrementCounter(counter.Id);
        await Task.Delay(50).ConfigureAwait(false);

        _viewModel.DecrementCounter(counter.Id);
        await Task.Delay(50).ConfigureAwait(false);

        counter.CurrentCount.Should().Be(0);
    }

    [Test]
    public async Task DecrementCounter_WhenAtZero_ShouldNotGoBelowZero()
    {
        var counter = MakeCounter();
        SetupLoadedProject(counters: [counter]);
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        _viewModel.DecrementCounter(counter.Id);
        await Task.Delay(50).ConfigureAwait(false);

        counter.CurrentCount.Should().Be(0);
    }

    [Test]
    public async Task DecrementCounter_ShouldTriggerHaptics()
    {
        var counter = MakeCounter();
        SetupLoadedProject(counters: [counter]);
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);
        _viewModel.IncrementCounter(counter.Id);
        await Task.Delay(50).ConfigureAwait(false);

        _viewModel.DecrementCounter(counter.Id);

        _mockHapticsService.Verify(x => x.Click(), Times.Exactly(2));
    }

    // ─── ResetCounterAsync ───────────────────────────────────────────────────

    [Test]
    public async Task ResetCounterAsync_WhenConfirmed_ShouldResetToZero()
    {
        var counter = MakeCounter();
        SetupLoadedProject(counters: [counter]);
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);
        _viewModel.IncrementCounter(counter.Id);
        _viewModel.IncrementCounter(counter.Id);
        await Task.Delay(50).ConfigureAwait(false);

        _mockDialogService
            .Setup(x => x.ShowConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        await _viewModel.ResetCounterAsync(counter.Id).ConfigureAwait(false);

        counter.CurrentCount.Should().Be(0);
    }

    [Test]
    public async Task ResetCounterAsync_WhenCancelled_ShouldNotReset()
    {
        var counter = MakeCounter();
        SetupLoadedProject(counters: [counter]);
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);
        _viewModel.IncrementCounter(counter.Id);
        _viewModel.IncrementCounter(counter.Id);
        await Task.Delay(50).ConfigureAwait(false);

        _mockDialogService
            .Setup(x => x.ShowConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        await _viewModel.ResetCounterAsync(counter.Id).ConfigureAwait(false);

        counter.CurrentCount.Should().Be(2);
    }

    // ─── UndoCounter ─────────────────────────────────────────────────────────

    [Test]
    public async Task UndoCounter_ShouldRevertLastIncrement()
    {
        var counter = MakeCounter();
        SetupLoadedProject(counters: [counter]);
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);
        _viewModel.IncrementCounter(counter.Id);
        _viewModel.IncrementCounter(counter.Id);
        await Task.Delay(50).ConfigureAwait(false);

        _viewModel.UndoCounter(counter.Id);
        await Task.Delay(50).ConfigureAwait(false);

        counter.CurrentCount.Should().Be(1);
    }

    [Test]
    public async Task UndoCounter_WhenNoHistory_ShouldNotCallRepository()
    {
        var counter = MakeCounter();
        SetupLoadedProject(counters: [counter]);
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        _viewModel.UndoCounter(counter.Id);
        await Task.Delay(50).ConfigureAwait(false);

        _mockCounterRepo.Verify(
            x => x.UpdateCountAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Guid>()),
            Times.Never);
    }

    [Test]
    public async Task UndoCounter_ShouldFireCountersChanged()
    {
        var counter = MakeCounter();
        SetupLoadedProject(counters: [counter]);
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);
        _viewModel.IncrementCounter(counter.Id);
        await Task.Delay(50).ConfigureAwait(false);

        var fired = false;
        _viewModel.CountersChanged += (_, _) => fired = true;
        _viewModel.UndoCounter(counter.Id);

        fired.Should().BeTrue();
    }

    // ─── AddCounterAsync ─────────────────────────────────────────────────────

    [Test]
    public async Task AddCounterAsync_ShouldAddCounterToList()
    {
        SetupLoadedProject(counters: [MakeCounter("Rows", 0)]);
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        await _viewModel.AddCounterAsync("Stitches").ConfigureAwait(false);

        _viewModel.Counters.Should().HaveCount(2);
        _viewModel.Counters.Should().Contain(c => c.Name == "Stitches");
    }

    [Test]
    public async Task AddCounterAsync_ShouldAssignIncrementingSortOrder()
    {
        SetupLoadedProject(counters: [MakeCounter("Rows", 0)]);
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        await _viewModel.AddCounterAsync("Stitches").ConfigureAwait(false);

        _viewModel.Counters.Should().Contain(c => c.Name == "Stitches" && c.SortOrder == 1);
    }

    [Test]
    public async Task AddCounterAsync_ShouldFireCountersChanged()
    {
        SetupLoadedProject();
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        var fired = false;
        _viewModel.CountersChanged += (_, _) => fired = true;

        await _viewModel.AddCounterAsync("Bobbles").ConfigureAwait(false);

        fired.Should().BeTrue();
    }

    [Test]
    public async Task AddCounterAsync_WithEmptyName_ShouldNotAdd()
    {
        SetupLoadedProject();
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);
        var countBefore = _viewModel.Counters.Count;

        await _viewModel.AddCounterAsync("").ConfigureAwait(false);

        _viewModel.Counters.Should().HaveCount(countBefore);
    }

    [Test]
    public async Task AddCounterAsync_WithWhitespaceName_ShouldNotAdd()
    {
        SetupLoadedProject();
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);
        var countBefore = _viewModel.Counters.Count;

        await _viewModel.AddCounterAsync("   ").ConfigureAwait(false);

        _viewModel.Counters.Should().HaveCount(countBefore);
    }

    // ─── DeleteCounterAsync ──────────────────────────────────────────────────

    [Test]
    public async Task DeleteCounterAsync_WhenOnlyOneCounter_ShouldShowAlertAndNotDelete()
    {
        var counter = MakeCounter();
        SetupLoadedProject(counters: [counter]);
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        await _viewModel.DeleteCounterAsync(counter.Id).ConfigureAwait(false);

        _mockDialogService.Verify(
            x => x.ShowAlertAsync("Cannot Delete", It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);
        _viewModel.Counters.Should().HaveCount(1);
    }

    [Test]
    public async Task DeleteCounterAsync_WhenConfirmed_ShouldRemoveCounter()
    {
        var primary = MakeCounter("Rows", 0);
        var secondary = MakeCounter("Stitches", 1);
        SetupLoadedProject(counters: [primary, secondary]);
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        _mockDialogService
            .Setup(x => x.ShowConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        await _viewModel.DeleteCounterAsync(secondary.Id).ConfigureAwait(false);

        _viewModel.Counters.Should().HaveCount(1);
        _viewModel.Counters.Should().NotContain(c => c.Name == "Stitches");
    }

    [Test]
    public async Task DeleteCounterAsync_WhenCancelled_ShouldNotRemoveCounter()
    {
        var primary = MakeCounter("Rows", 0);
        var secondary = MakeCounter("Stitches", 1);
        SetupLoadedProject(counters: [primary, secondary]);
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        _mockDialogService
            .Setup(x => x.ShowConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        await _viewModel.DeleteCounterAsync(secondary.Id).ConfigureAwait(false);

        _viewModel.Counters.Should().HaveCount(2);
    }

    [Test]
    public async Task DeleteCounterAsync_WhenConfirmed_ShouldFireCountersChanged()
    {
        var primary = MakeCounter("Rows", 0);
        var secondary = MakeCounter("Stitches", 1);
        SetupLoadedProject(counters: [primary, secondary]);
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        _mockDialogService
            .Setup(x => x.ShowConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        var fired = false;
        _viewModel.CountersChanged += (_, _) => fired = true;

        await _viewModel.DeleteCounterAsync(secondary.Id).ConfigureAwait(false);

        fired.Should().BeTrue();
    }

    // ─── Session ─────────────────────────────────────────────────────────────

    [Test]
    public async Task ToggleSession_WhenNotRunning_ShouldStartSession()
    {
        SetupLoadedProject();
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        _viewModel.ToggleSessionCommand.Execute(null);

        _viewModel.IsSessionRunning.Should().BeTrue();
    }

    [Test]
    public async Task ToggleSession_WhenRunning_ShouldPauseSession()
    {
        SetupLoadedProject();
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);
        _viewModel.ToggleSessionCommand.Execute(null); // start

        _viewModel.ToggleSessionCommand.Execute(null); // pause

        _viewModel.IsSessionRunning.Should().BeFalse();
    }

    [Test]
    public async Task ToggleSession_ShouldUpdateSessionButtonText()
    {
        SetupLoadedProject();
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        _viewModel.SessionButtonText.Should().Be("START SESSION");
        _viewModel.ToggleSessionCommand.Execute(null);
        _viewModel.SessionButtonText.Should().Be("PAUSE");
    }

    [Test]
    public async Task ToggleSession_ShouldUpdateSessionButtonIcon()
    {
        SetupLoadedProject();
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        _viewModel.SessionButtonIcon.Should().Be("play.svg");
        _viewModel.ToggleSessionCommand.Execute(null);
        _viewModel.SessionButtonIcon.Should().Be("pause.svg");
    }

    [Test]
    public async Task EndSession_WhenSessionWasStarted_ShouldPersistSession()
    {
        SetupLoadedProject();
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);
        _viewModel.ToggleSessionCommand.Execute(null);

        _viewModel.EndSessionCommand.Execute(null);
        await Task.Delay(200).ConfigureAwait(false);

        _mockSessionRepo.Verify(x => x.AddAsync(It.IsAny<Session>()), Times.Once);
        _mockSessionRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task EndSession_ShouldNavigateBack()
    {
        SetupLoadedProject();
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        _viewModel.EndSessionCommand.Execute(null);
        await Task.Delay(200).ConfigureAwait(false);

        _mockNavigationService.Verify(x => x.GoBackAsync(), Times.Once);
    }

    [Test]
    public async Task EndSession_WhenNoSessionStarted_ShouldNotPersistSession()
    {
        SetupLoadedProject();
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);
        // deliberately NOT starting a session

        _viewModel.EndSessionCommand.Execute(null);
        await Task.Delay(200).ConfigureAwait(false);

        _mockSessionRepo.Verify(x => x.AddAsync(It.IsAny<Session>()), Times.Never);
    }

    [Test]
    public async Task EndSession_ShouldShowToast()
    {
        SetupLoadedProject();
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        _viewModel.EndSessionCommand.Execute(null);
        await Task.Delay(200).ConfigureAwait(false);

        _mockDialogService.Verify(x => x.ShowToastAsync(It.IsAny<string>()), Times.Once);
    }

    // ─── Progress properties ─────────────────────────────────────────────────

    [Test]
    public async Task ProgressValue_WithNoTotalRows_ShouldBeZero()
    {
        SetupLoadedProject();
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        _viewModel.ProgressValue.Should().Be(0);
    }

    [Test]
    public async Task ProgressValue_ShouldBeProportionOfTotalRows()
    {
        _project.UpdateProjectDetails(totalRows: 10);
        var counter = MakeCounter();
        SetupLoadedProject(counters: [counter]);
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        _viewModel.IncrementCounter(counter.Id);
        _viewModel.IncrementCounter(counter.Id);
        _viewModel.IncrementCounter(counter.Id);
        _viewModel.IncrementCounter(counter.Id);
        _viewModel.IncrementCounter(counter.Id); // 5 of 10
        await Task.Delay(50).ConfigureAwait(false);

        _viewModel.ProgressValue.Should().BeApproximately(0.5, precision: 0.001);
    }

    [Test]
    public async Task ProgressValue_ShouldNotExceedOne()
    {
        _project.UpdateProjectDetails(totalRows: 5);
        var counter = MakeCounter();
        SetupLoadedProject(counters: [counter]);
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        for (int i = 0; i < 10; i++)
            _viewModel.IncrementCounter(counter.Id);
        await Task.Delay(50).ConfigureAwait(false);

        _viewModel.ProgressValue.Should().BeLessThanOrEqualTo(1.0);
    }

    [Test]
    public async Task ProgressText_WithTotalRows_ShouldShowRowsFormat()
    {
        _project.UpdateProjectDetails(totalRows: 20);
        var counter = MakeCounter();
        SetupLoadedProject(counters: [counter]);
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        _viewModel.IncrementCounter(counter.Id);
        await Task.Delay(50).ConfigureAwait(false);

        _viewModel.ProgressText.Should().Be("1 / 20 rows");
    }

    [Test]
    public async Task ProgressText_WithoutTotalRows_ShouldShowRowNumberFormat()
    {
        var counter = MakeCounter();
        SetupLoadedProject(counters: [counter]);
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        _viewModel.IncrementCounter(counter.Id);
        await Task.Delay(50).ConfigureAwait(false);

        _viewModel.ProgressText.Should().Be("Row 1");
    }

    // ─── Session timer ───────────────────────────────────────────────────────

    [TestCase(0, 0, 45, "45s")]
    [TestCase(0, 3, 20, "3m 20s")]
    [TestCase(1, 15, 0, "1h 15m")]
    public void UpdateSessionTimer_ShouldFormatDurationCorrectly(int hours, int minutes, int seconds, string expected)
    {
        _viewModel.UpdateSessionTimer(new TimeSpan(hours, minutes, seconds));

        _viewModel.SessionTimerText.Should().Be(expected);
    }

    // ─── Row notes ───────────────────────────────────────────────────────────

    [Test]
    public async Task AddRowNoteAsync_ShouldAddNoteToCollection()
    {
        SetupLoadedProject();
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        await _viewModel.AddRowNoteAsync(12, "decrease here").ConfigureAwait(false);

        _viewModel.RowNotes.Should().HaveCount(1);
        _viewModel.RowNotes[0].RowNumber.Should().Be(12);
        _viewModel.RowNotes[0].NoteText.Should().Be("decrease here");
    }

    [Test]
    public async Task AddRowNoteAsync_ShouldPersistToRepository()
    {
        SetupLoadedProject();
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        await _viewModel.AddRowNoteAsync(5, "switch colour").ConfigureAwait(false);

        _mockRowNoteRepo.Verify(x => x.AddAsync(It.IsAny<RowNote>()), Times.Once);
        _mockRowNoteRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task AddRowNoteAsync_ShouldFireRowNotesChanged()
    {
        SetupLoadedProject();
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        var fired = false;
        _viewModel.RowNotesChanged += (_, _) => fired = true;

        await _viewModel.AddRowNoteAsync(8, "start sleeve").ConfigureAwait(false);

        fired.Should().BeTrue();
    }

    [Test]
    public async Task AddRowNoteAsync_ShouldKeepNotesOrderedByRowNumber()
    {
        SetupLoadedProject();
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        await _viewModel.AddRowNoteAsync(20, "late note").ConfigureAwait(false);
        await _viewModel.AddRowNoteAsync(5, "early note").ConfigureAwait(false);

        _viewModel.RowNotes[0].RowNumber.Should().Be(5);
        _viewModel.RowNotes[1].RowNumber.Should().Be(20);
    }

    [Test]
    public async Task DeleteRowNoteAsync_ShouldRemoveNoteFromCollection()
    {
        SetupLoadedProject();
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);
        await _viewModel.AddRowNoteAsync(10, "temp note").ConfigureAwait(false);
        var noteId = _viewModel.RowNotes[0].Id;

        await _viewModel.DeleteRowNoteAsync(noteId).ConfigureAwait(false);

        _viewModel.RowNotes.Should().BeEmpty();
    }

    [Test]
    public async Task DeleteRowNoteAsync_ShouldCallRepository()
    {
        SetupLoadedProject();
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);
        await _viewModel.AddRowNoteAsync(10, "temp note").ConfigureAwait(false);
        var noteId = _viewModel.RowNotes[0].Id;

        await _viewModel.DeleteRowNoteAsync(noteId).ConfigureAwait(false);

        _mockRowNoteRepo.Verify(x => x.DeleteAsync(noteId), Times.Once);
    }

    // ─── Notes expand/collapse ───────────────────────────────────────────────

    [Test]
    public async Task ToggleNotesCommand_ShouldExpandNotes()
    {
        SetupLoadedProject();
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        var maxLinesBefore = _viewModel.NotesMaxLines;
        _viewModel.ToggleNotesCommand.Execute(null);

        _viewModel.NotesMaxLines.Should().NotBe(maxLinesBefore);
        _viewModel.NotesToggleText.Should().Contain("less");
    }

    [Test]
    public async Task ToggleNotesCommand_WhenExpanded_ShouldCollapse()
    {
        SetupLoadedProject();
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);
        _viewModel.ToggleNotesCommand.Execute(null); // expand

        _viewModel.ToggleNotesCommand.Execute(null); // collapse

        _viewModel.NotesToggleText.Should().Contain("all");
    }

    // ─── PropertyChanged ─────────────────────────────────────────────────────

    [Test]
    public async Task IncrementPrimaryCounter_ShouldRaisePropertyChanged_ForCurrentCount()
    {
        var counter = MakeCounter("Rows", 0);
        SetupLoadedProject(counters: [counter]);
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        var changed = new List<string?>();
        _viewModel.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        _viewModel.IncrementCounter(counter.Id);

        changed.Should().Contain(nameof(_viewModel.CurrentCount));
    }

    [Test]
    public async Task IncrementSecondaryCounter_ShouldNotRaisePropertyChanged_ForCurrentCount()
    {
        var primary = MakeCounter("Rows", 0);
        var secondary = MakeCounter("Stitches", 1);
        SetupLoadedProject(counters: [primary, secondary]);
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        var changed = new List<string?>();
        _viewModel.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        _viewModel.IncrementCounter(secondary.Id);

        changed.Should().NotContain(nameof(_viewModel.CurrentCount));
    }

    [Test]
    public async Task ToggleSession_ShouldRaisePropertyChanged_ForIsSessionRunning()
    {
        SetupLoadedProject();
        await _viewModel.LoadProjectAsync().ConfigureAwait(false);

        var changed = new List<string?>();
        _viewModel.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        _viewModel.ToggleSessionCommand.Execute(null);

        changed.Should().Contain(nameof(_viewModel.IsSessionRunning));
    }
}
