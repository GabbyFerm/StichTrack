// Copyright (c) 2026 Gabriella Frank Ferm / Frank Ferm Design. All rights reserved.
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using StitchTrack.Application.Commands;
using StitchTrack.Application.Interfaces;
using StitchTrack.Domain.Entities;
using StitchTrack.Domain.Interfaces;

namespace StitchTrack.Application.ViewModels;

/// <summary>
/// ViewModel for ProjectCounterPage.
/// Manages a list of named counters — each with independent count, undo, and reset.
/// The primary counter (SortOrder == 0) drives progress, sessions, and Project.CurrentCount.
///
/// Flow:
/// - Load → fetches project + counters from DB, fires CountersChanged so page builds cards
/// - Each counter card in code-behind calls Increment/Decrement/Reset/UndoCounterAsync(id)
/// - Save / End Session → persists all counter counts, ends the session
/// </summary>

public class ProjectCounterViewModel : INotifyPropertyChanged
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectCounterRepository _counterRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IRowNoteRepository _rowNoteRepository;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    private readonly IHapticsService _hapticsService = null!;

    private Project? _project;
    private List<ProjectCounter> _counters = new();
    private Session? _currentSession;
    private bool _isSessionRunning;
    private TimeSpan _sessionDuration;
    private bool _notesExpanded;
    private List<RowNote> _rowNotes = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    // Fired when counters are added, removed, or counts change — page rebuilds all counter cards in response
    public event EventHandler? CountersChanged;
    public event EventHandler? RowNotesChanged;

    public Guid ProjectId { get; set; }

    // ─── Project properties ──────────────────────────────────────

    public string ProjectName => _project?.Name ?? "Project";
    public string? ColorHex => _project?.ColorHex;
    public int? TotalRows => _project?.TotalRows;
    public string? Notes => _project?.Notes;
    public bool HasNotes => !string.IsNullOrWhiteSpace(Notes);
    public bool HasTotalRows => TotalRows.HasValue && TotalRows > 0;
    public bool HasRowNotes => _rowNotes.Count > 0;

    // ─── Counters ────────────────────────────────────────────────

    public IReadOnlyList<ProjectCounter> Counters => _counters.AsReadOnly();

    // Primary counter drives progress display and sessions
    private ProjectCounter? PrimaryCounter =>
        _counters.FirstOrDefault(c => c.SortOrder == 0);

    // CurrentCount reflects the primary counter for progress bar / ProgressText
    public int CurrentCount => PrimaryCounter?.CurrentCount ?? 0;

    // ─── Progress (driven by primary counter) ────────────────────

    public double ProgressValue =>
        HasTotalRows && TotalRows!.Value > 0
            ? Math.Min((double)CurrentCount / TotalRows.Value, 1.0)
            : 0;

    public string ProgressText =>
        HasTotalRows
            ? $"{CurrentCount} / {TotalRows} rows"
            : $"Row {CurrentCount}";

    public string ProgressPercentage =>
        HasTotalRows && TotalRows!.Value > 0
            ? $"{(int)(ProgressValue * 100)}% done"
            : string.Empty;

    // ─── Pattern files ───────────────────────────────────────────

    public bool HasPattern =>
        _project?.ProjectFiles.Any(f => f.FileType == ProjectFileType.Pattern) ?? false;

    public IReadOnlyList<ProjectFile> PatternFiles =>
        _project?.ProjectFiles
            .Where(f => f.FileType == ProjectFileType.Pattern)
            .OrderByDescending(f => f.UploadedAt)
            .ToList() ?? [];

    public Func<string, Task>? OpenFileAsync { get; set; }

    // ─── Session / Timer ─────────────────────────────────────────

    public bool IsSessionRunning
    {
        get => _isSessionRunning;
        private set
        {
            if (_isSessionRunning == value) return;
            _isSessionRunning = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SessionButtonText));
            OnPropertyChanged(nameof(SessionButtonIcon));
        }
    }

    public string SessionTimerText => FormatDuration(_sessionDuration);
    public string SessionButtonText => IsSessionRunning ? "PAUSE" : "START SESSION";
    public string SessionButtonIcon => IsSessionRunning ? "pause.svg" : "play.svg";

    // ─── Notes expand/collapse ───────────────────────────────────

    public bool NotesCanExpand => HasNotes && (Notes?.Split('\n').Length ?? 0) > 6;
    public int NotesMaxLines => _notesExpanded ? int.MaxValue : 6;
    public string NotesToggleText => _notesExpanded ? "See less ▲" : "See all notes ▼";

    // ─── Page-level commands (session, save, notes) ──────────────

    public ICommand ToggleSessionCommand { get; }
    public ICommand SaveProgressCommand { get; }
    public ICommand EndSessionCommand { get; }
    public ICommand ToggleNotesCommand { get; }

    public Func<Task>? OnNavigateBack { get; set; }

    public ProjectCounterViewModel(
    IProjectRepository projectRepository,
    IProjectCounterRepository counterRepository,
    IRowNoteRepository rowNoteRepository,
    ISessionRepository sessionRepository,
    IDialogService dialogService,
    INavigationService navigationService,
    IHapticsService hapticsService)
    {
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        _counterRepository = counterRepository ?? throw new ArgumentNullException(nameof(counterRepository));
        _rowNoteRepository = rowNoteRepository ?? throw new ArgumentNullException(nameof(rowNoteRepository));
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _hapticsService = hapticsService ?? throw new ArgumentNullException(nameof(hapticsService));

        ToggleSessionCommand = new RelayCommand(OnToggleSession);
        SaveProgressCommand = new RelayCommand(OnSaveProgress);
        EndSessionCommand = new RelayCommand(OnEndSession);
        ToggleNotesCommand = new RelayCommand(OnToggleNotes);

    }

    // ─── Load ─────────────────────────────────────────────────────

    public async Task LoadProjectAsync()
    {
        try
        {

            _project = await _projectRepository
                .GetByIdWithoutHistoryAsync(ProjectId)
                .ConfigureAwait(false);

            if (_project == null)
            {
                await _dialogService.ShowAlertAsync("Error", "Project not found").ConfigureAwait(false);
                return;
            }

            _counters = (await _counterRepository
                .GetByProjectIdAsync(ProjectId)
                .ConfigureAwait(false)).ToList();

            _rowNotes = (await _rowNoteRepository
                .GetByProjectIdAsync(ProjectId)
                .ConfigureAwait(false)).ToList();

            OnPropertyChanged(string.Empty);
            OnPropertyChanged(nameof(HasRowNotes));
            OnPropertyChanged(nameof(HasPattern));

            CountersChanged?.Invoke(this, EventArgs.Empty);
            RowNotesChanged?.Invoke(this, EventArgs.Empty);

        }
#pragma warning disable CA1031
        catch (Exception)
#pragma warning restore CA1031
        {
            await _dialogService.ShowAlertAsync("Error", "Could not load project").ConfigureAwait(false);
        }
    }

    // ─── Per-counter actions (called from code-behind with counter ID) ──

    /// <summary>Increments a specific counter. Fires CountersChanged for UI rebuild.</summary>
    public async void IncrementCounter(Guid counterId)
    {
        var counter = _counters.FirstOrDefault(c => c.Id == counterId);
        if (counter == null) return;

        counter.Increment();
        _hapticsService.Click();
        NotifyCounterChanged(counter);

        await _counterRepository.UpdateCountAsync(
            counter.Id, counter.CurrentCount,
            isPrimary: counter.SortOrder == 0,
            projectId: _project!.Id)
            .ConfigureAwait(false);
    }

    /// <summary>Decrements a specific counter. No-op if already at 0.</summary>
    public async void DecrementCounter(Guid counterId)
    {
        var counter = _counters.FirstOrDefault(c => c.Id == counterId);
        if (counter == null) return;

        counter.Decrement();
        _hapticsService.Click();
        NotifyCounterChanged(counter);

        await _counterRepository.UpdateCountAsync(
            counter.Id, counter.CurrentCount,
            isPrimary: counter.SortOrder == 0,
            projectId: _project!.Id)
            .ConfigureAwait(false);
    }

    /// <summary>Resets a specific counter to 0 after confirmation.</summary>
    public async Task ResetCounterAsync(Guid counterId)
    {
        var counter = _counters.FirstOrDefault(c => c.Id == counterId);
        if (counter == null) return;

        var confirmed = await _dialogService.ShowConfirmAsync(
            title: "Reset Counter?",
            message: $"Reset '{counter.Name}' to 0? Saved progress is unaffected until you save.",
            accept: "Reset",
            cancel: "Cancel")
            .ConfigureAwait(true);

        if (!confirmed) return;

        counter.Reset();
        NotifyCounterChanged(counter);

        await _counterRepository.UpdateCountAsync(
            counter.Id, counter.CurrentCount,
            isPrimary: counter.SortOrder == 0,
            projectId: _project!.Id)
            .ConfigureAwait(false);
    }

    /// <summary>Undoes the last change on a specific counter using its in-memory history.</summary>
    public async void UndoCounter(Guid counterId)
    {
        var counter = _counters.FirstOrDefault(c => c.Id == counterId);
        if (counter == null) return;

        var undone = counter.UndoLastChange();
        if (!undone) return;

        NotifyCounterChanged(counter);

        await _counterRepository.UpdateCountAsync(
            counter.Id, counter.CurrentCount,
            isPrimary: counter.SortOrder == 0,
            projectId: _project!.Id)
            .ConfigureAwait(false);
    }

    // ─── Counter management ──────────────────────────────────────

    /// <summary>Adds a new named counter. Fires CountersChanged for page rebuild.</summary>
    public async Task AddCounterAsync(string name)
    {
        if (_project == null || string.IsNullOrWhiteSpace(name)) return;

        var sortOrder = _counters.Count > 0
            ? _counters.Max(c => c.SortOrder) + 1
            : 0;

        var counter = ProjectCounter.Create(_project.Id, name.Trim(), sortOrder);

        await _counterRepository.AddAsync(counter).ConfigureAwait(false);
        await _counterRepository.SaveChangesAsync().ConfigureAwait(false);

        _counters.Add(counter);
        CountersChanged?.Invoke(this, EventArgs.Empty);

    }

    /// <summary>Deletes a counter after confirmation. Primary counter cannot be deleted if it's the only one.</summary>
    public async Task DeleteCounterAsync(Guid counterId)
    {
        var counter = _counters.FirstOrDefault(c => c.Id == counterId);
        if (counter == null) return;

        if (_counters.Count == 1)
        {
            await _dialogService.ShowAlertAsync(
                "Cannot Delete",
                "A project must have at least one counter.")
                .ConfigureAwait(false);
            return;
        }

        var confirmed = await _dialogService.ShowConfirmAsync(
            title: "Delete Counter?",
            message: $"Delete '{counter.Name}' and all its history? This cannot be undone.",
            accept: "Delete",
            cancel: "Cancel")
            .ConfigureAwait(true);

        if (!confirmed) return;

        await _counterRepository.DeleteAsync(counterId).ConfigureAwait(false);
        _counters.Remove(counter);
        CountersChanged?.Invoke(this, EventArgs.Empty);

    }

    // ─── Session ─────────────────────────────────────────────────

    private void OnToggleSession()
    {
        if (IsSessionRunning) PauseSession(); else StartSession();
    }

    private void StartSession()
    {
        if (_project == null) return;

        // Pass primary counter name so session history shows the correct label
        _currentSession ??= Session.StartSession(
            _project.Id,
            PrimaryCounter?.CurrentCount,
            PrimaryCounter?.Name);

        IsSessionRunning = true;
    }

    private void PauseSession()
    {
        IsSessionRunning = false;
    }

    public void UpdateSessionTimer(TimeSpan elapsed)
    {
        _sessionDuration = elapsed;
        OnPropertyChanged(nameof(SessionTimerText));
    }

    // ─── Save & End ──────────────────────────────────────────────

    private void OnSaveProgress() => _ = SaveProgressAsync();
    private void OnEndSession() => _ = EndSessionAsync();

    /// <summary>Saves all counter counts. Stays on the page.</summary>
    public async Task SaveProgressAsync()
    {
        if (_project == null) return;
        try
        {
            await SaveAllCountersAsync().ConfigureAwait(false);
            await _dialogService
                .ShowToastAsync($"Progress saved — row {CurrentCount}")
                .ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch (Exception)
#pragma warning restore CA1031
        {
            await _dialogService.ShowAlertAsync("Save Failed", "Could not save progress.").ConfigureAwait(false);
        }
    }

    /// <summary>Silently saves all counter counts on page disappear (swipe back etc.).</summary>
    public async Task AutoSaveAsync()
    {
        if (_project == null) return;
        try
        {
            await SaveAllCountersAsync().ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch (Exception)
#pragma warning restore CA1031
        {
        }
    }

    private async Task EndSessionAsync()
    {
        if (_project == null) return;
        try
        {
            await SaveAllCountersAsync().ConfigureAwait(false);

            if (_currentSession != null)
            {
                _currentSession.EndSession(PrimaryCounter?.CurrentCount);
                await _sessionRepository.AddAsync(_currentSession).ConfigureAwait(false);
                await _sessionRepository.SaveChangesAsync().ConfigureAwait(false);

                await _dialogService
                    .ShowToastAsync($"Session saved — row {CurrentCount}")
                    .ConfigureAwait(false);
            }
            else
            {
                await _dialogService
                    .ShowToastAsync($"Progress saved — row {CurrentCount}")
                    .ConfigureAwait(false);
            }

            IsSessionRunning = false;
            await _navigationService.GoBackAsync().ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch (Exception)
#pragma warning restore CA1031
        {
            await _dialogService.ShowAlertAsync("Error", "Could not save session.").ConfigureAwait(false);
        }
    }

    /// <summary>Persists all counter counts. Primary counter also syncs Project.CurrentCount.</summary>
    private async Task SaveAllCountersAsync()
    {
        foreach (var counter in _counters)
        {
            await _counterRepository.UpdateCountAsync(
                counter.Id,
                counter.CurrentCount,
                isPrimary: counter.SortOrder == 0,
                projectId: _project!.Id)
                .ConfigureAwait(false);
        }
    }

    // ─── Notes ───────────────────────────────────────────────────

    private void OnToggleNotes()
    {
        _notesExpanded = !_notesExpanded;
        OnPropertyChanged(nameof(NotesMaxLines));
        OnPropertyChanged(nameof(NotesToggleText));
    }

    public async Task AddRowNoteAsync(int rowNumber, string noteText)
    {
        if (_project == null) return;

        var note = RowNote.CreateRowNote(_project.Id, rowNumber, noteText);
        await _rowNoteRepository.AddAsync(note).ConfigureAwait(false);
        await _rowNoteRepository.SaveChangesAsync().ConfigureAwait(false);

        _rowNotes.Add(note);
        _rowNotes = _rowNotes.OrderBy(rn => rn.RowNumber).ToList();

        OnPropertyChanged(nameof(HasRowNotes));
        RowNotesChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task DeleteRowNoteAsync(Guid noteId)
    {
        await _rowNoteRepository.DeleteAsync(noteId).ConfigureAwait(false);
        _rowNotes.RemoveAll(rn => rn.Id == noteId);
        OnPropertyChanged(nameof(HasRowNotes));
        RowNotesChanged?.Invoke(this, EventArgs.Empty);
    }

    public ReadOnlyCollection<RowNote> RowNotes => _rowNotes.AsReadOnly();

    // ─── Helpers ─────────────────────────────────────────────────
    /// <summary>Notifies progress properties for a specific counter change.</summary>
    private void NotifyCounterChanged(ProjectCounter counter)
    {
        // Always notify CountersChanged so code-behind can refresh the count display
        CountersChanged?.Invoke(this, EventArgs.Empty);

        // Only notify progress properties when the primary counter changes
        if (counter.SortOrder == 0)
        {
            OnPropertyChanged(nameof(CurrentCount));
            OnPropertyChanged(nameof(ProgressValue));
            OnPropertyChanged(nameof(ProgressText));
            OnPropertyChanged(nameof(ProgressPercentage));
        }
    }

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalHours >= 1)
            return $"{(int)duration.TotalHours}h {duration.Minutes}m";
        if (duration.TotalMinutes >= 1)
            return $"{duration.Minutes}m {duration.Seconds}s";
        return $"{duration.Seconds}s";
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
