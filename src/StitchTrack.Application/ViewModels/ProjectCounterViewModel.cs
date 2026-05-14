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
/// Handles counting rows against an existing project, with optional session tracking.
///
/// Flow:
/// - User taps Play  → starts visual timer + creates in-memory Session
/// - User taps Pause → pauses visual timer (session keeps its start time)
/// - Save Progress   → saves Project.CurrentCount only, stays on page
/// - End Session     → saves Session + Project.CurrentCount, navigates back
/// </summary>
public class ProjectCounterViewModel : INotifyPropertyChanged
{
    private readonly IProjectRepository _projectRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IRowNoteRepository _rowNoteRepository;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    private readonly IHapticsService _hapticsService = null!;

    private Project? _project;
    private Session? _currentSession;
    private bool _isSessionRunning;
    private TimeSpan _sessionDuration;
    private bool _notesExpanded;
    private List<RowNote> _rowNotes = new();

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? RowNotesChanged;

    // Project ID set from Shell navigation query parameter
    public Guid ProjectId { get; set; }

    // ─── Project properties ──────────────────────────────────────

    public string ProjectName => _project?.Name ?? "Project";
    public string? ColorHex => _project?.ColorHex;
    public int CurrentCount => _project?.CurrentCount ?? 0;
    public int? TotalRows => _project?.TotalRows;
    public string? Notes => _project?.Notes;
    public bool HasNotes => !string.IsNullOrWhiteSpace(Notes);
    public bool HasTotalRows => TotalRows.HasValue && TotalRows > 0;
    public bool HasRowNotes => _rowNotes.Count > 0;

    // ─── Progress ────────────────────────────────────────────────

    // Progress 0.0–1.0 for the progress bar
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

    // ─── Session / Timer ─────────────────────────────────────────

    public bool IsSessionRunning
    {
        get => _isSessionRunning;
        private set
        {
            if (_isSessionRunning != value)
            {
                _isSessionRunning = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SessionButtonText));
                OnPropertyChanged(nameof(SessionButtonIcon));
            }
        }
    }

    // Timer duration formatted for display e.g. "1h 23m" or "4m 12s"
    public string SessionTimerText => FormatDuration(_sessionDuration);

    public string SessionButtonText => IsSessionRunning ? "PAUSE" : "START SESSION";
    public string SessionButtonIcon => IsSessionRunning ? "pause.svg" : "play.svg";

    // ─── Pattern ─────────────────────────────────────────────────
    public bool HasPattern =>
    (_project?.ProjectFiles.Any(f => f.FileType == ProjectFileType.Pattern) ?? false);

    public string PatternFileName =>
        _project?.ProjectFiles
            .FirstOrDefault(f => f.FileType == ProjectFileType.Pattern)?.FileName
            ?? string.Empty;

    private string? PatternFilePath =>
        _project?.ProjectFiles
            .FirstOrDefault(f => f.FileType == ProjectFileType.Pattern)?.FilePath;

    public IReadOnlyList<ProjectFile> PatternFiles =>
        _project?.ProjectFiles
            .Where(f => f.FileType == ProjectFileType.Pattern)
            .OrderByDescending(f => f.UploadedAt)
            .ToList() ?? [];

    // Set by the Page — same pattern as SingleProjectViewModel
    public Func<string, Task>? OpenFileAsync { get; set; }

    // ─── Notes expand/collapse ───────────────────────────────────

    public bool NotesCanExpand => HasNotes && (Notes?.Split('\n').Length ?? 0) > 6;
    public int NotesMaxLines => _notesExpanded ? int.MaxValue : 6;
    public string NotesToggleText => _notesExpanded ? "See less ▲" : "See all notes ▼";

    // ─── Commands ────────────────────────────────────────────────

    public ICommand IncrementCommand { get; }
    public ICommand DecrementCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand ToggleSessionCommand { get; }
    public ICommand SaveProgressCommand { get; }
    public ICommand EndSessionCommand { get; }
    public ICommand ToggleNotesCommand { get; }
    public ICommand UndoCommand { get; }
    public ICommand ViewPatternCommand { get; }

    // Set by the Page to handle navigation back
    public Func<Task>? OnNavigateBack { get; set; }

    public ProjectCounterViewModel(
        IProjectRepository projectRepository,
        IRowNoteRepository rowNoteRepository,
        ISessionRepository sessionRepository,
        IDialogService dialogService,
        INavigationService navigationService,
        IHapticsService hapticsService)
    {
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        _rowNoteRepository = rowNoteRepository ?? throw new ArgumentNullException(nameof(rowNoteRepository));
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _hapticsService = hapticsService ?? throw new ArgumentNullException(nameof(hapticsService));

        IncrementCommand = new RelayCommand(OnIncrement);
        DecrementCommand = new RelayCommand(OnDecrement);
        ResetCommand = new RelayCommand(OnReset);
        ToggleSessionCommand = new RelayCommand(OnToggleSession);
        SaveProgressCommand = new RelayCommand(OnSaveProgress);
        EndSessionCommand = new RelayCommand(OnEndSession);
        ToggleNotesCommand = new RelayCommand(OnToggleNotes);
        UndoCommand = new RelayCommand(OnUndo);
        ViewPatternCommand = new RelayCommand(OnViewPattern);

        System.Diagnostics.Debug.WriteLine("✅ ProjectCounterViewModel created");
    }

    /// <summary>
    /// Loads the project from the database using ProjectId set by navigation.
    /// </summary>
    public async Task LoadProjectAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"📂 Loading project for counter: {ProjectId}");

            _project = await _projectRepository.GetByIdWithoutHistoryAsync(ProjectId).ConfigureAwait(false);

            if (_project == null)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Project not found: {ProjectId}");
                await _dialogService.ShowAlertAsync("Error", "Project not found").ConfigureAwait(false);
                return;
            }

            OnPropertyChanged(string.Empty);
            _rowNotes = (await _rowNoteRepository
                .GetByProjectIdAsync(ProjectId)
                .ConfigureAwait(false)).ToList();

            OnPropertyChanged(nameof(HasRowNotes));
            RowNotesChanged?.Invoke(this, EventArgs.Empty);

            OnPropertyChanged(nameof(HasPattern));
            OnPropertyChanged(nameof(PatternFileName));

            System.Diagnostics.Debug.WriteLine($"✅ Project loaded for counter: {_project.Name} (row {_project.CurrentCount})");
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error loading project: {ex.Message}");
            await _dialogService.ShowAlertAsync("Error", "Could not load project").ConfigureAwait(false);
        }
    }

    // ─── Counter actions ─────────────────────────────────────────

    private void OnIncrement()
    {
        if (_project == null) return;
        _project.IncrementCount();
        _hapticsService.Click();
        NotifyCounterChanged();
    }

    private void OnDecrement()
    {
        if (_project == null) return;
        _project.DecrementCount();
        _hapticsService.Click();
        NotifyCounterChanged();
    }

    private async void OnReset()
    {
        if (_project == null) return;

        // Reset is destructive in context — confirm before clearing progress
        var confirmed = await _dialogService.ShowConfirmAsync(
            title: "Reset Counter?",
            message: "This will reset the counter to 0. Your saved progress won't be affected until you save.",
            accept: "Reset",
            cancel: "Cancel"
        ).ConfigureAwait(true);

        if (!confirmed) return;

        _project.ResetCount();
        NotifyCounterChanged();
    }

    /// <summary>
    /// Notifies all counter-related properties so the UI updates.
    /// </summary>
    private void NotifyCounterChanged()
    {
        OnPropertyChanged(nameof(CurrentCount));
        OnPropertyChanged(nameof(ProgressValue));
        OnPropertyChanged(nameof(ProgressText));
        OnPropertyChanged(nameof(ProgressPercentage));
    }

    // ─── Session actions ─────────────────────────────────────────

    private void OnToggleSession()
    {
        if (IsSessionRunning)
            PauseSession();
        else
            StartSession();
    }

    private void StartSession()
    {
        if (_project == null) return;

        // Create session in memory — only persisted on EndSession
        _currentSession ??= Session.StartSession(_project.Id, _project.CurrentCount);

        IsSessionRunning = true;
        System.Diagnostics.Debug.WriteLine($"▶️ Session started for {_project.Name}");
    }

    private void PauseSession()
    {
        IsSessionRunning = false;
        System.Diagnostics.Debug.WriteLine("⏸️ Session paused");
    }

    /// <summary>
    /// Updates the session timer display — called by the Page's timer tick.
    /// Kept here so the ViewModel controls the formatted display string.
    /// </summary>
    public void UpdateSessionTimer(TimeSpan elapsed)
    {
        _sessionDuration = elapsed;
        OnPropertyChanged(nameof(SessionTimerText));
    }

    // ─── Save & End ──────────────────────────────────────────────

    private void OnSaveProgress() => _ = SaveProgressAsync();
    private void OnEndSession() => _ = EndSessionAsync();

    /// <summary>
    /// Saves the current count to the database.
    /// Stays on the page — session keeps running.
    /// </summary>
    public async Task SaveProgressAsync()
    {
        if (_project == null) return;

        try
        {
            // Direct SQL update — bypasses change tracker entirely
            await _projectRepository.UpdateCountAsync(
                _project.Id,
                _project.CurrentCount,
                DateTime.UtcNow
            ).ConfigureAwait(false);

            await _dialogService.ShowToastAsync($"Progress saved — row {_project.CurrentCount}").ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error saving progress: {ex.Message}");
            await _dialogService.ShowAlertAsync("Save Failed", "Could not save progress.").ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Silently saves the current count when the page disappears.
    /// Covers swipe back and other navigation gestures that bypass OnBackButtonPressed.
    /// </summary>
    public async Task AutoSaveAsync()
    {
        if (_project == null) return;

        try
        {
            await _projectRepository.UpdateCountAsync(
                _project.Id,
                _project.CurrentCount,
                DateTime.UtcNow
            ).ConfigureAwait(false);

            System.Diagnostics.Debug.WriteLine($"💾 Auto-saved count: {_project.CurrentCount}");
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            System.Diagnostics.Debug.WriteLine($"❌ Auto-save failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Ends the session: saves count + session record, then navigates back.
    /// If no session was started, just saves the count and goes back.
    /// </summary>
    private async Task EndSessionAsync()
    {
        if (_project == null) return;

        try
        {
            // Direct SQL update — bypasses change tracker entirely
            await _projectRepository.UpdateCountAsync(
                _project.Id,
                _project.CurrentCount,
                DateTime.UtcNow
            ).ConfigureAwait(false);

            // Save the session record if one was started
            if (_currentSession != null)
            {
                _currentSession.EndSession(_project.CurrentCount);
                await _sessionRepository.AddAsync(_currentSession).ConfigureAwait(false);
                await _sessionRepository.SaveChangesAsync().ConfigureAwait(false);

                System.Diagnostics.Debug.WriteLine($"✅ Session ended: {_currentSession.DurationSeconds}s, rows {_currentSession.StartingRowCount}→{_currentSession.EndingRowCount}");

                await _dialogService.ShowToastAsync($"Session saved — row {_project.CurrentCount}").ConfigureAwait(false);
            }
            else
            {
                await _dialogService.ShowToastAsync($"Progress saved — row {_project.CurrentCount}").ConfigureAwait(false);
            }

            IsSessionRunning = false;
            await _navigationService.GoBackAsync().ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error ending session: {ex.Message}");
            await _dialogService.ShowAlertAsync("Error", "Could not save session.").ConfigureAwait(false);
        }
    }

    // ─── Notes ───────────────────────────────────────────────────

    private void OnToggleNotes()
    {
        _notesExpanded = !_notesExpanded;
        OnPropertyChanged(nameof(NotesMaxLines));
        OnPropertyChanged(nameof(NotesToggleText));
    }

    private void OnUndo()
    {
        if (_project == null) return;

        var undone = _project.UndoLastChange();
        if (undone)
            NotifyCounterChanged();
    }

    /// <summary>
    /// Saves a new row note and notifies the Page to rebuild the grid.
    /// Called directly from the Page code-behind on the add button tap.
    /// </summary>
    public async Task AddRowNoteAsync(int rowNumber, string noteText)
    {
        if (_project == null) return;

        var note = RowNote.CreateRowNote(_project.Id, rowNumber, noteText);

        await _rowNoteRepository.AddAsync(note).ConfigureAwait(false);
        await _rowNoteRepository.SaveChangesAsync().ConfigureAwait(false);

        // Re-sort in memory so the grid displays in row number order
        _rowNotes.Add(note);
        _rowNotes = _rowNotes.OrderBy(rn => rn.RowNumber).ToList();

        OnPropertyChanged(nameof(HasRowNotes));
        RowNotesChanged?.Invoke(this, EventArgs.Empty);

        System.Diagnostics.Debug.WriteLine($"✅ Row note added: row {rowNumber} — {noteText}");
    }

    /// <summary>
    /// Deletes a row note by ID and notifies the Page to rebuild the grid.
    /// ExecuteDeleteAsync in the repository handles the SQL directly — no SaveChanges needed.
    /// </summary>
    public async Task DeleteRowNoteAsync(Guid noteId)
    {
        await _rowNoteRepository.DeleteAsync(noteId).ConfigureAwait(false);

        _rowNotes.RemoveAll(rn => rn.Id == noteId);

        OnPropertyChanged(nameof(HasRowNotes));
        RowNotesChanged?.Invoke(this, EventArgs.Empty);

        System.Diagnostics.Debug.WriteLine($"🗑️ Row note deleted: {noteId}");
    }

    public System.Collections.ObjectModel.ReadOnlyCollection<RowNote> RowNotes
    => _rowNotes.AsReadOnly();

    // ─── Helpers ─────────────────────────────────────────────────
    private async void OnViewPattern()
    {
        if (string.IsNullOrWhiteSpace(PatternFilePath) || OpenFileAsync == null) return;
        await OpenFileAsync(PatternFilePath).ConfigureAwait(false);
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
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
