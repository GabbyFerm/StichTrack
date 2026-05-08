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
    private readonly IPatternFileRepository _patternFileRepository;
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

    // ─── Pattern ──────────────────────────────────────────────────

    public bool HasPattern => (_project?.PatternFiles.Count ?? 0) > 0;

    // Shows the file name of the first pattern — tappable link in the UI
    public string PatternFileName
    {
        get
        {
            var pattern = _project?.PatternFiles.FirstOrDefault();
            return pattern != null ? pattern.FileName : string.Empty;
        }
    }

    // Full local path used by ViewPatternCommand to open the file
    private string? PatternFilePath => _project?.PatternFiles.FirstOrDefault()?.FilePath;

    /// <summary>
    /// Set by SingleProjectPage to open a file in the native viewer.
    /// </summary>
    public Func<string, Task>? OpenFileAsync { get; set; }

    // ─── Notes ────────────────────────────────────────────────────

    public bool HasNotes => !string.IsNullOrWhiteSpace(Notes);
    public bool NotesCanExpand => HasNotes && (Notes?.Split('\n').Length ?? 0) > 3;
    public int NotesMaxLines => _notesExpanded ? int.MaxValue : 3;
    public string NotesToggleText => _notesExpanded ? "See less ▲" : "See all notes ▼";

    /// <summary>
    /// Set by SingleProjectPage to open the create/edit form popup.
    /// </summary>
    public Func<Project?, Task<ProjectFormResult?>>? ShowProjectFormAsync { get; set; }

    // ─── Commands ────────────────────────────────────────────────

    public ICommand ContinueCountingCommand { get; }
    public ICommand ViewPatternCommand { get; }
    public ICommand ToggleNotesCommand { get; }
    public ICommand EditProjectCommand { get; }
    public ICommand ArchiveProjectCommand { get; }
    public ICommand DeleteProjectCommand { get; }

    public SingleProjectViewModel(
        IProjectRepository projectRepository,
        IPatternFileRepository patternFileRepository,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        _patternFileRepository = patternFileRepository ?? throw new ArgumentNullException(nameof(patternFileRepository));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

        ContinueCountingCommand = new RelayCommand(OnContinueCounting);
        ViewPatternCommand = new RelayCommand(OnViewPattern);
        ToggleNotesCommand = new RelayCommand(OnToggleNotes);
        EditProjectCommand = new RelayCommand(OnEditProject);
        ArchiveProjectCommand = new RelayCommand(OnArchiveProject);
        DeleteProjectCommand = new RelayCommand(OnDeleteProject);

        System.Diagnostics.Debug.WriteLine("✅ SingleProjectViewModel created");
    }

    // ─── Load ─────────────────────────────────────────────────────

    public async Task LoadProjectAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"📂 Loading project: {ProjectId}");

            // GetByIdAsync includes PatternFiles so HasPattern and PatternFileName work
            _project = await _projectRepository.GetByIdAsync(ProjectId).ConfigureAwait(false);

            if (_project == null)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Project not found: {ProjectId}");
                await _dialogService.ShowAlertAsync("Error", "Project not found").ConfigureAwait(false);
                return;
            }

            // Reset expand state on each load so notes display correctly after edit
            _notesExpanded = false;

            OnPropertyChanged(string.Empty);
            System.Diagnostics.Debug.WriteLine($"✅ Project loaded: {_project.Name}");
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error loading project: {ex.Message}");
            await _dialogService.ShowAlertAsync("Error", "Could not load project").ConfigureAwait(false);
        }
    }

    // ─── Edit ─────────────────────────────────────────────────────

    private async Task EditProjectAsync()
    {
        if (_project == null) return;

        if (ShowProjectFormAsync == null)
        {
            System.Diagnostics.Debug.WriteLine("⚠️ ShowProjectFormAsync callback not set");
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
                notes: result.Notes
            );

            // Save image if a new one was picked
            if (!string.IsNullOrWhiteSpace(result.ImagePath))
                _project.SetProjectImage(result.ImagePath);

            await _projectRepository.UpdateAsync(_project).ConfigureAwait(false);
            await _projectRepository.SaveChangesAsync().ConfigureAwait(false);

            // Save pattern file if a new one was picked
            if (!string.IsNullOrWhiteSpace(result.PatternFilePath))
            {
                var fileName = result.PatternFileName ?? Path.GetFileName(result.PatternFilePath!);
                var fileSize = new FileInfo(result.PatternFilePath).Length;
                var extension = Path.GetExtension(result.PatternFilePath);
                var contentType = extension.ToUpperInvariant() switch
                {
                    ".jpg" => "image/jpeg",
                    ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".pdf" => "application/pdf",
                    _ => "application/octet-stream"
                };
                var pattern = PatternFile.CreatePatternFile(
                    _project.Id, fileName, result.PatternFilePath, fileSize, contentType);

                await _patternFileRepository.AddAsync(pattern).ConfigureAwait(false);
                await _patternFileRepository.SaveChangesAsync().ConfigureAwait(false);
            }

            // Reload to get fresh data including any new pattern file
            await LoadProjectAsync().ConfigureAwait(false);

            System.Diagnostics.Debug.WriteLine($"✅ Project updated: {_project.Name}");
            await _dialogService.ShowToastAsync($"'{_project.Name}' updated!").ConfigureAwait(false);
        }
        catch (InvalidOperationException ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error updating project: {ex.Message}");
            await _dialogService.ShowAlertAsync("Update Failed", "Could not save changes.").ConfigureAwait(false);
        }
        catch (ArgumentException ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Validation error: {ex.Message}");
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

            System.Diagnostics.Debug.WriteLine($"📦 Project archived: {_project.Name}");
            await _dialogService.ShowToastAsync($"'{_project.Name}' archived").ConfigureAwait(false);
            await _navigationService.GoBackAsync().ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error archiving: {ex.Message}");
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
            System.Diagnostics.Debug.WriteLine($"❌ Error unarchiving: {ex.Message}");
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

            System.Diagnostics.Debug.WriteLine($"🗑️ Project deleted: {_project.Name}");
            await _dialogService.ShowToastAsync($"'{_project.Name}' deleted").ConfigureAwait(false);
            await _navigationService.GoBackAsync().ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error deleting: {ex.Message}");
            await _dialogService.ShowAlertAsync("Delete Failed", "Could not delete project.").ConfigureAwait(false);
        }
    }

    // ─── Pattern viewer ──────────────────────────────────────────

    private async void OnViewPattern()
    {
        if (string.IsNullOrWhiteSpace(PatternFilePath))
        {
            System.Diagnostics.Debug.WriteLine("⚠️ No pattern file path");
            return;
        }

        if (OpenFileAsync == null)
        {
            System.Diagnostics.Debug.WriteLine("⚠️ OpenFileAsync callback not set");
            return;
        }

        await OpenFileAsync(PatternFilePath).ConfigureAwait(false);
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
}
