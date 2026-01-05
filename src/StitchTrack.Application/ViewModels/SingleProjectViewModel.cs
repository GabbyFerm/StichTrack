using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using StitchTrack.Application.Commands;
using StitchTrack.Application.Interfaces;
using StitchTrack.Domain.Entities;
using StitchTrack.Domain.Interfaces;

namespace StitchTrack.Application.ViewModels;

public class SingleProjectViewModel : INotifyPropertyChanged
{
    private readonly IProjectRepository _projectRepository;
    private readonly IDialogService _dialogService;
    private Project? _project;
    private bool _notesExpanded;

    public event PropertyChangedEventHandler? PropertyChanged;

    // Project ID (set from navigation)
    public Guid ProjectId { get; set; }

    // Project properties
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

    // Commands
    public ICommand SyncCommand { get; }
    public ICommand ShowMenuCommand { get; }
    public ICommand ContinueCountingCommand { get; }
    public ICommand ViewPatternCommand { get; }
    public ICommand ToggleNotesCommand { get; }
    public ICommand EditProjectCommand { get; }
    public ICommand ArchiveProjectCommand { get; }

    public SingleProjectViewModel(
        IProjectRepository projectRepository,
        IDialogService dialogService)
    {
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        // Initialize commands
        SyncCommand = new RelayCommand(OnSync);
        ShowMenuCommand = new RelayCommand(OnShowMenu);
        ContinueCountingCommand = new RelayCommand(OnContinueCounting);
        ViewPatternCommand = new RelayCommand(OnViewPattern);
        ToggleNotesCommand = new RelayCommand(OnToggleNotes);
        EditProjectCommand = new RelayCommand(OnEditProject);
        ArchiveProjectCommand = new RelayCommand(OnArchiveProject);

        System.Diagnostics.Debug.WriteLine("✅ SingleProjectViewModel created");
    }

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

    private void OnSync()
    {
        System.Diagnostics.Debug.WriteLine("🔄 Sync tapped");
        // TODO: Implement sync
    }

    private void OnShowMenu()
    {
        System.Diagnostics.Debug.WriteLine("⋮ Menu tapped");
        // TODO: Show action sheet with Edit, Archive, Delete
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
        System.Diagnostics.Debug.WriteLine("✏️ Edit Project tapped");
        // TODO: Show edit modal
    }

    private void OnArchiveProject()
    {
        System.Diagnostics.Debug.WriteLine("🗑️ Archive Project tapped");
        _ = ArchiveProjectAsync();
    }

    private async Task ArchiveProjectAsync()
    {
        if (_project == null) return;

        var confirmed = await _dialogService.ShowPromptAsync(
            title: "Archive Project? ",
            message: $"Are you sure you want to archive '{_project.Name}'?  Type 'ARCHIVE' to confirm.",
            accept: "Archive",
            cancel: "Cancel",
            placeholder: "ARCHIVE",
            maxLength: 10
        ).ConfigureAwait(false);

        if (confirmed?.Equals("ARCHIVE", StringComparison.OrdinalIgnoreCase) != true)
        {
            return;
        }

        try
        {
            _project.ArchiveProject();
            await _projectRepository.UpdateAsync(_project).ConfigureAwait(false);
            await _projectRepository.SaveChangesAsync().ConfigureAwait(false);

            await _dialogService.ShowToastAsync($"Project '{_project.Name}' archived").ConfigureAwait(false);

            // Navigate back to projects list
            // TODO: await Shell.Current.GoToAsync(". .");
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception ex)
#pragma warning restore CA1031
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error archiving project: {ex.Message}");
            await _dialogService.ShowAlertAsync("Error", "Could not archive project").ConfigureAwait(false);
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
