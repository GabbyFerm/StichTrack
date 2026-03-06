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
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    private Project? _project;
    private bool _notesExpanded;

    public event PropertyChangedEventHandler? PropertyChanged;

    // Project ID set from Shell navigation query parameter
    public Guid ProjectId { get; set; }

    // Project properties exposed to the UI
    public string ProjectName => _project?.Name ?? "Project";
    public string? ImagePath => _project?.ImagePath;
    public string? ColorHex => _project?.ColorHex;
    public int CurrentCount => _project?.CurrentCount ?? 0;
    public int? TotalRows => _project?.TotalRows;
    public string? Notes => _project?.Notes;
    public string ImageUrl
    {
        get
        {
            if (_project == null)
                return "project_placeholder.jpg";

            // If project has an image URL, use it
            if (!string.IsNullOrWhiteSpace(_project.ImageUrl))
                return _project.ImageUrl;

            // If project has a local image path, use it
            if (!string.IsNullOrWhiteSpace(_project.ImagePath))
                return _project.ImagePath;

            // Otherwise use placeholder
            return "project_placeholder. jpg";
        }
    }

    public bool HasImage
    {
        get
        {
            if (_project == null)
                return true; // Show placeholder

            return !string.IsNullOrWhiteSpace(_project.ImageUrl) ||
                   !string.IsNullOrWhiteSpace(_project.ImagePath);
        }
    }

    // UI state
    public bool HasPattern => (_project?.PatternFiles.Count ?? 0) > 0;
    public bool HasNotes => !string.IsNullOrWhiteSpace(Notes);
    public bool NotesCanExpand => HasNotes && (Notes?.Length ?? 0) > 150;
    public int NotesMaxLines => _notesExpanded ? int.MaxValue : 3;
    public string NotesToggleText => _notesExpanded ? "See less ▲" : "See all notes ▼";
    public bool IsArchived => _project?.IsArchived ?? false;
    public string ArchiveButtonText => IsArchived ? "Unarchive" : "Archive";

    /// <summary>
    /// Set by SingleProjectPage to open the create/edit form popup.
    /// Receives the project to edit. Returns the result or null if cancelled.
    /// </summary>
    public Func<Project?, Task<ProjectFormResult?>>? ShowProjectFormAsync { get; set; }

    // Commands
    public ICommand SyncCommand { get; }
    public ICommand ContinueCountingCommand { get; }
    public ICommand ViewPatternCommand { get; }
    public ICommand ToggleNotesCommand { get; }
    public ICommand EditProjectCommand { get; }
    public ICommand ArchiveProjectCommand { get; }
    public ICommand DeleteProjectCommand { get; }

    public SingleProjectViewModel(
        IProjectRepository projectRepository,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

        SyncCommand = new RelayCommand(OnSync);
        ContinueCountingCommand = new RelayCommand(OnContinueCounting);
        ViewPatternCommand = new RelayCommand(OnViewPattern);
        ToggleNotesCommand = new RelayCommand(OnToggleNotes);
        EditProjectCommand = new RelayCommand(OnEditProject);
        ArchiveProjectCommand = new RelayCommand(OnArchiveProject);
        DeleteProjectCommand = new RelayCommand(OnDeleteProject);

        System.Diagnostics.Debug.WriteLine("✅ SingleProjectViewModel created");
    }

    /// <summary>
    /// Loads the project from the database using ProjectId set by navigation.
    /// </summary>
    public async Task LoadProjectAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"📂 Loading project: {ProjectId}");

            _project = await _projectRepository.GetByIdAsync(ProjectId).ConfigureAwait(false);

            if (_project == null)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Project not found: {ProjectId}");
                await _dialogService.ShowAlertAsync("Error", "Project not found").ConfigureAwait(false);
                return;
            }

            // Notify all properties
            OnPropertyChanged(string.Empty);

            System.Diagnostics.Debug.WriteLine($"✅ Project loaded: {_project.Name}");
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception ex)
#pragma warning restore CA1031
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error loading project: {ex.Message}");
            await _dialogService.ShowAlertAsync("Error", "Could not load project").ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Opens the edit form popup pre-filled with the current project data.
    /// </summary>
    private async Task EditProjectAsync()
    {
        if (_project == null) return;

        if (ShowProjectFormAsync == null)
        {
            System.Diagnostics.Debug.WriteLine("⚠️ ShowProjectFormAsync callback not set");
            return;
        }

        var result = await ShowProjectFormAsync(_project).ConfigureAwait(true);

        if (result == null)
        {
            System.Diagnostics.Debug.WriteLine("⚠️ Edit cancelled");
            return;
        }

        try
        {
            // Entity controls all state changes via domain methods
            _project.Rename(result.Name);
            _project.UpdateProjectDetails(
                colorHex: result.ColorHex,
                totalRows: result.TotalRows,
                notes: result.Notes
            );

            await _projectRepository.UpdateAsync(_project).ConfigureAwait(false);
            await _projectRepository.SaveChangesAsync().ConfigureAwait(false);

            // Refresh all UI properties to reflect the changes
            OnPropertyChanged(string.Empty);

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

    /// <summary>
    /// Archives the project and navigates back to the projects list.
    /// </summary>
    private async Task ArchiveProjectAsync()
    {
        if (_project == null) return;

        try
        {
            await _projectRepository.ArchiveAsync(_project.Id).ConfigureAwait(false);
            await _projectRepository.SaveChangesAsync().ConfigureAwait(false);

            System.Diagnostics.Debug.WriteLine($"📦 Project archived: {_project.Name}");
            await _dialogService.ShowToastAsync($"'{_project.Name}' archived").ConfigureAwait(false);

            // Go back to the projects list
            await _navigationService.GoBackAsync().ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error archiving project: {ex.Message}");
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

    /// <summary>
    /// Permanently deletes the project after explicit confirmation, then navigates back.
    /// </summary>
    private async Task DeleteProjectAsync()
    {
        if (_project == null) return;

        // Require typing DELETE — permanent action deserves a higher bar
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

            // Go back to the projects list
            await _navigationService.GoBackAsync().ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error deleting project: {ex.Message}");
            await _dialogService.ShowAlertAsync("Delete Failed", "Could not delete project.").ConfigureAwait(false);
        }
    }
    private void OnSync()
    {
        System.Diagnostics.Debug.WriteLine("🔄 Sync tapped");
        // TODO: Implement sync
    }

    private void OnContinueCounting()
    {
        System.Diagnostics.Debug.WriteLine("▶️ Continue Counting tapped");
        // TODO: Navigate to counter page with project context
    }

    private void OnViewPattern()
    {
        System.Diagnostics.Debug.WriteLine("📄 View Pattern tapped");
        // TODO: Open PDF viewer
    }

    private void OnToggleNotes()
    {
        _notesExpanded = !_notesExpanded;
        OnPropertyChanged(nameof(NotesMaxLines));
        OnPropertyChanged(nameof(NotesToggleText));
    }

    private void OnEditProject()
    {
        _ = EditProjectAsync();
    }

    private void OnArchiveProject()
    {
        _ = IsArchived ? UnarchiveProjectAsync() : ArchiveProjectAsync();
    }
    private void OnDeleteProject()
    {
        _ = DeleteProjectAsync();
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
