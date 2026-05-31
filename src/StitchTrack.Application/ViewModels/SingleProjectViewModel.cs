// Copyright (c) 2026 Gabriella Frank Ferm / Frank Ferm Design. All rights reserved.
using StitchTrack.Application.Commands;
using StitchTrack.Application.Interfaces;
using StitchTrack.Application.Models;
using StitchTrack.Domain.Entities;
using StitchTrack.Domain.Interfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace StitchTrack.Application.ViewModels;

public class SingleProjectViewModel : INotifyPropertyChanged
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectFileRepository _projectFileRepository;
    private readonly IProjectCounterRepository _counterRepository;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    private Project? _project;
    private bool _notesExpanded;

    public event PropertyChangedEventHandler? PropertyChanged;

    public Guid ProjectId { get; set; }

    // ─── Project properties ──────────────────────────────────────

    public string ProjectName => _project?.Name ?? "Project";
    public string? ColorHex => _project?.ColorHex;
    public int CurrentCount => _project?.CurrentCount ?? 0;
    public int? TotalRows => _project?.TotalRows;
    public string? Notes => _project?.Notes;
    public bool IsArchived => _project?.IsArchived ?? false;
    public string ArchiveButtonText => IsArchived ? "Unarchive" : "Archive";

    public string ImageUrl
    {
        get
        {
            if (_project == null) return "project_placeholder.jpg";
            if (!string.IsNullOrWhiteSpace(_project.ImageUrl)) return _project.ImageUrl;
            if (!string.IsNullOrWhiteSpace(_project.ImagePath)) return _project.ImagePath;
            return "project_placeholder.jpg"; // fixed typo — no space
        }
    }

    public string ProgressPercentage
    {
        get
        {
            if (_project?.TotalRows == null || _project.TotalRows == 0) return string.Empty;
            var pct = (int)Math.Min((double)CurrentCount / _project.TotalRows.Value * 100, 100);
            return $"{pct}% done";
        }
    }

    // ─── Project Files ────────────────────────────────────────────
    /// <summary>
    /// Checks if the project has any pattern-type files.
    /// </summary>
    public bool HasPatternFiles =>
        (_project?.ProjectFiles.Any(f => f.FileType == ProjectFileType.Pattern) ?? false);

    public bool HasInspirationPhotos =>
    (_project?.ProjectFiles.Any(f => f.FileType == ProjectFileType.InspirationPhoto) ?? false);

    public IReadOnlyList<ProjectFile> PatternFiles =>
    _project?.ProjectFiles
        .Where(f => f.FileType == ProjectFileType.Pattern)
        .OrderByDescending(f => f.UploadedAt)
        .ToList() ?? [];

    public IReadOnlyList<ProjectFile> InspirationPhotos =>
    _project?.ProjectFiles
        .Where(f => f.FileType == ProjectFileType.InspirationPhoto)
        .OrderByDescending(f => f.UploadedAt)
        .ToList() ?? [];

    /// <summary>
    /// Set by SingleProjectPage — callback to open a file (pattern, inspiration photo, etc.) in the native viewer.
    /// Receives a local file path and handles platform-specific file opening.
    /// </summary>
    public Func<string, Task>? OpenFileAsync { get; set; }

    // ─── Notes ────────────────────────────────────────────────────

    public bool HasNotes => !string.IsNullOrWhiteSpace(Notes);
    public bool NotesCanExpand => HasNotes && (Notes?.Split('\n').Length ?? 0) > 3;
    public int NotesMaxLines => _notesExpanded ? int.MaxValue : 3;
    public string NotesToggleText => _notesExpanded ? "See less ▲" : "See all notes ▼";

    // ─── Tags & size ──────────────────────────────────────────────

    public IReadOnlyList<ProjectTag> Tags =>
        _project?.Tags.OrderBy(t => t.ColorIndex).ToList() ?? [];

    public bool HasTags => (_project?.Tags.Count ?? 0) > 0;

    public string? NeedleOrHookSize => _project?.NeedleOrHookSize;
    public bool HasNeedleOrHookSize => !string.IsNullOrWhiteSpace(_project?.NeedleOrHookSize);

    public IReadOnlyList<ProjectCounter> Counters =>
    _project?.Counters.OrderBy(c => c.SortOrder).ToList() ?? [];

    public bool HasCounters => (_project?.Counters.Count ?? 0) > 0;

    /// <summary>
    /// Set by SingleProjectPage — callback to display the project edit form popup.
    /// Returns the updated form result with any changes (tags, files, details), or null if cancelled.
    /// </summary>
    public Func<Project?, Task<ProjectFormResult?>>? ShowProjectFormAsync { get; set; }

    // ─── Session summary ──────────────────────────────────────────

    public bool HasSessions => (_project?.Sessions.Count ?? 0) > 0;

    // "Today", "Yesterday", or "14 May"
    public string LastSessionText
    {
        get
        {
            var last = _project?.Sessions
                .OrderByDescending(s => s.StartedAt)
                .FirstOrDefault();

            if (last == null) return string.Empty;

            var local = last.StartedAt.ToLocalTime();

            if (local.Date == DateTime.Today) return "Today";
            if (local.Date == DateTime.Today.AddDays(-1)) return "Yesterday";

            return local.ToString("dd MMM", System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    // "3h 20m", "45m", etc.
    public string TotalTimeText
    {
        get
        {
            if (_project == null || _project.Sessions.Count == 0) return string.Empty;

            var total = TimeSpan.FromSeconds(_project.Sessions.Sum(s => s.DurationSeconds));

            if (total.TotalHours >= 1)
                return $"{(int)total.TotalHours}h {total.Minutes}m";
            if (total.TotalMinutes >= 1)
                return $"{(int)total.TotalMinutes}m";

            return $"{(int)total.TotalSeconds}s";
        }
    }

    // ─── Commands ────────────────────────────────────────────────

    public ICommand ContinueCountingCommand { get; }
    public ICommand ToggleNotesCommand { get; }
    public ICommand EditProjectCommand { get; }
    public ICommand ArchiveProjectCommand { get; }
    public ICommand DeleteProjectCommand { get; }

    public SingleProjectViewModel(
        IProjectRepository projectRepository,
        IProjectFileRepository projectFileRepository,
        IProjectCounterRepository counterRepository,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        _projectFileRepository = projectFileRepository ?? throw new ArgumentNullException(nameof(projectFileRepository));
        _counterRepository = counterRepository ?? throw new ArgumentNullException(nameof(counterRepository));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

        ContinueCountingCommand = new RelayCommand(OnContinueCounting);
        ToggleNotesCommand = new RelayCommand(OnToggleNotes);
        EditProjectCommand = new RelayCommand(OnEditProject);
        ArchiveProjectCommand = new RelayCommand(OnArchiveProject);
        DeleteProjectCommand = new RelayCommand(OnDeleteProject);

    }

    // ─── Load ─────────────────────────────────────────────────────

    public async Task LoadProjectAsync()
    {
        try
        {

            _project = await _projectRepository.GetByIdAsync(ProjectId).ConfigureAwait(false);

            if (_project == null)
            {
                await _dialogService.ShowAlertAsync("Error", "Project not found").ConfigureAwait(false);
                return;
            }

            // Reset expand state on each load so notes display correctly after edit
            _notesExpanded = false;

            OnPropertyChanged(string.Empty);
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            await _dialogService.ShowAlertAsync("Error", "Could not load project").ConfigureAwait(false);
        }
    }

    // ─── Edit ─────────────────────────────────────────────────────

    private async Task EditProjectAsync()
    {
        if (_project == null) return;

        if (ShowProjectFormAsync == null)
        {
            return;
        }

        var result = await ShowProjectFormAsync(_project).ConfigureAwait(true);
        if (result == null) return;

        try
        {
            _project.Rename(result.Name);
            _project.UpdateProjectDetails(
                colorHex: result.ColorHex,
                totalRows: result.TotalRows,
                notes: result.Notes,
                needleOrHookSize: result.NeedleOrHookSize
            );

            // Save image if a new one was picked
            if (!string.IsNullOrWhiteSpace(result.ImagePath))
                _project.SetProjectImage(result.ImagePath);

            await _projectRepository.UpdateAsync(_project).ConfigureAwait(false);
            await _projectRepository.SaveChangesAsync().ConfigureAwait(false);

            // Sync tags — delete all existing and re-insert (avoids change tracking issues)
            await _projectRepository.UpdateTagsAsync(_project.Id, result.Tags).ConfigureAwait(false);
            await _projectRepository.SaveChangesAsync().ConfigureAwait(false);

            // Sync project files (pattern files, inspiration photos, etc.) with the database
            await SyncProjectFilesAsync(_project.Id, result.ProjectFiles, _projectFileRepository)
                    .ConfigureAwait(false);

            // Reload to get fresh data including any newly attached project files
            await LoadProjectAsync().ConfigureAwait(false);

            await _dialogService.ShowToastAsync($"'{_project.Name}' updated!").ConfigureAwait(false);
        }
        catch (InvalidOperationException ex)
        {
            await _dialogService.ShowAlertAsync("Update Failed", "Could not save changes.").ConfigureAwait(false);
        }
        catch (ArgumentException ex)
        {
            await _dialogService.ShowAlertAsync("Invalid Input", ex.Message).ConfigureAwait(false);
        }
    }

    // ─── Archive / Unarchive ─────────────────────────────────────

    private async Task ArchiveProjectAsync()
    {
        if (_project == null) return;

        try
        {
            await _projectRepository.ArchiveAsync(_project.Id).ConfigureAwait(false);
            await _projectRepository.SaveChangesAsync().ConfigureAwait(false);

            await _dialogService.ShowToastAsync($"'{_project.Name}' archived").ConfigureAwait(false);
            await _navigationService.GoBackAsync().ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            await _dialogService.ShowAlertAsync("Archive Failed", "Could not archive project.").ConfigureAwait(false);
        }
    }

    private async Task UnarchiveProjectAsync()
    {
        if (_project == null) return;

        try
        {
            _project.UnarchiveProject();
            await _projectRepository.UpdateAsync(_project).ConfigureAwait(false);
            await _projectRepository.SaveChangesAsync().ConfigureAwait(false);

            await _dialogService.ShowToastAsync($"'{_project.Name}' restored").ConfigureAwait(false);
            await _navigationService.GoBackAsync().ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            await _dialogService.ShowAlertAsync("Restore Failed", "Could not restore project.").ConfigureAwait(false);
        }
    }

    // ─── Delete ──────────────────────────────────────────────────

    private async Task DeleteProjectAsync()
    {
        if (_project == null) return;

        var result = await _dialogService.ShowPromptAsync(
            title: "Delete Project?",
            message: $"This will permanently delete '{_project.Name}'. Type DELETE to confirm.",
            accept: "Delete",
            cancel: "Cancel",
            placeholder: "DELETE",
            maxLength: 10
        ).ConfigureAwait(true);

        if (result?.Equals("DELETE", StringComparison.OrdinalIgnoreCase) != true) return;

        try
        {
            await _projectRepository.DeleteAsync(_project.Id).ConfigureAwait(false);
            await _projectRepository.SaveChangesAsync().ConfigureAwait(false);

            await _dialogService.ShowToastAsync($"'{_project.Name}' deleted").ConfigureAwait(false);
            await _navigationService.GoBackAsync().ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            await _dialogService.ShowAlertAsync("Delete Failed", "Could not delete project.").ConfigureAwait(false);
        }
    }

    // ─── Pattern viewer ──────────────────────────────────────────

    public async Task OpenProjectFileAsync(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || OpenFileAsync == null) return;
        await OpenFileAsync(filePath).ConfigureAwait(false);
    }

    /// <summary>
    /// Adds a named counter to the project and reloads.
    /// </summary>
    public async Task AddCounterAsync(string name)
    {
        if (_project == null || string.IsNullOrWhiteSpace(name)) return;

        var sortOrder = _project.Counters.Count > 0
            ? _project.Counters.Max(c => c.SortOrder) + 1
            : 0;

        var counter = ProjectCounter.Create(_project.Id, name.Trim(), sortOrder);
        await _counterRepository.AddAsync(counter).ConfigureAwait(false);
        await _counterRepository.SaveChangesAsync().ConfigureAwait(false);

        await LoadProjectAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes a counter after confirmation. Blocked if it's the last one.
    /// </summary>
    public async Task RemoveCounterAsync(Guid counterId)
    {
        if (_project == null) return;

        if (_project.Counters.Count <= 1)
        {
            await _dialogService.ShowAlertAsync(
                "Cannot Delete", "A project must have at least one counter.")
                .ConfigureAwait(false);
            return;
        }

        var counter = _project.Counters.FirstOrDefault(c => c.Id == counterId);
        if (counter == null) return;

        var confirmed = await _dialogService.ShowConfirmAsync(
            title: "Delete Counter?",
            message: $"Delete '{counter.Name}' and all its history? This cannot be undone.",
            accept: "Delete",
            cancel: "Cancel")
            .ConfigureAwait(true);

        if (!confirmed) return;

        await _counterRepository.DeleteAsync(counterId).ConfigureAwait(false);
        await LoadProjectAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Renames a counter and reloads to reflect the change.
    /// </summary>
    public async Task RenameCounterAsync(Guid counterId, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName)) return;

        await _counterRepository.RenameAsync(counterId, newName).ConfigureAwait(false);
        await LoadProjectAsync().ConfigureAwait(false);
    }

    // ─── Other handlers ──────────────────────────────────────────

    private async void OnContinueCounting()
    {
        if (_project == null) return;
        await _navigationService.NavigateToAsync(
            $"ProjectCounterPage?ProjectId={_project.Id}"
        ).ConfigureAwait(false);
    }

    private void OnToggleNotes()
    {
        _notesExpanded = !_notesExpanded;
        OnPropertyChanged(nameof(NotesMaxLines));
        OnPropertyChanged(nameof(NotesToggleText));
    }

    private void OnEditProject() => _ = EditProjectAsync();
    private void OnArchiveProject() => _ = IsArchived ? UnarchiveProjectAsync() : ArchiveProjectAsync();
    private void OnDeleteProject() => _ = DeleteProjectAsync();

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Syncs the project file list from the form result against the database.
    /// Deletes removed files, inserts new ones.
    /// </summary>
    private static async Task SyncProjectFilesAsync(
        Guid projectId,
        IReadOnlyList<PendingProjectFile> pendingFiles,
        IProjectFileRepository projectFileRepository)
    {
        // Load what's currently in the DB for this project
        var existingFiles = (await projectFileRepository
            .GetByProjectIdAsync(projectId)
            .ConfigureAwait(false)).ToList();

        // IDs that the user kept in the form
        var keptIds = pendingFiles
            .Where(f => f.ExistingId.HasValue)
            .Select(f => f.ExistingId!.Value)
            .ToHashSet();

        // Delete files that were removed in the form
        foreach (var removed in existingFiles.Where(f => !keptIds.Contains(f.Id)))
        {
            await projectFileRepository.DeleteAsync(removed.Id).ConfigureAwait(false);

            // Also remove the physical file if it exists locally
            if (!string.IsNullOrWhiteSpace(removed.FilePath) && File.Exists(removed.FilePath))
                File.Delete(removed.FilePath);

        }

        // Add new files (those without an existing DB ID)
        foreach (var newFile in pendingFiles.Where(f => f.ExistingId == null))
        {
            var file = ProjectFile.Create(
                projectId,
                newFile.FileName,
                newFile.FilePath,
                newFile.FileSizeBytes,
                newFile.FileType,
                newFile.ContentType);

            await projectFileRepository.AddAsync(file).ConfigureAwait(false);
        }

        if (pendingFiles.Any(f => f.ExistingId == null) || existingFiles.Any(f => !keptIds.Contains(f.Id)))
            await projectFileRepository.SaveChangesAsync().ConfigureAwait(false);
    }

}
