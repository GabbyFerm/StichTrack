using StitchTrack.Application.Commands;
using StitchTrack.Application.Interfaces;
using StitchTrack.Application.Models;
using StitchTrack.Domain.Entities;
using StitchTrack.Domain.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace StitchTrack.Application.ViewModels;

/// <summary>
/// ViewModel for the Projects list page.
/// Manages the list of projects, filtering between active and archived.
/// </summary>
public class ProjectsViewModel : INotifyPropertyChanged
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectFileRepository _projectFileRepository;
    private readonly IProjectCounterRepository _counterRepository;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    private readonly SynchronizationContext? _syncContext;
    private bool _isLoading;
    private bool _isEmpty;
    private bool _showArchived;
    private string _searchText = string.Empty;
    private bool _isSearchVisible;

    public event PropertyChangedEventHandler? PropertyChanged;

    // Holds every project loaded from the database (both active and archived)
    private readonly ObservableCollection<Project> _allProjects = new();

    // The filtered list the UI binds to
    public ObservableCollection<Project> Projects { get; } = new();

    // Controls which tab is active — triggers a re-filter when changed
    public bool ShowArchived
    {
        get => _showArchived;
        set
        {
            if (_showArchived != value)
            {
                _showArchived = value;
                OnPropertyChanged();
                FilterProjects();
            }
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
    }

    // Search text from the search bar. Filtering is applied on every keystroke.
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value) return;
            _searchText = value;
            OnPropertyChanged();
            FilterProjects(); // filter on every keystroke
        }
    }

    public bool IsSearchVisible
    {
        get => _isSearchVisible;
        set
        {
            if (_isSearchVisible == value) return;
            _isSearchVisible = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsSearchNotVisible));
        }
    }

    // Used in XAML to hide the normal header when search is open
    public bool IsSearchNotVisible => !_isSearchVisible;

    // When true the empty state view is shown instead of the list
    public bool IsEmpty
    {
        get => _isEmpty;
        private set
        {
            if (_isEmpty != value)
            {
                _isEmpty = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasProjects));
            }
        }
    }

    public bool HasProjects => !IsEmpty;

    // Tab labels include live counts so they stay in sync after changes
    public string ActiveProjectsTabText => $"Active projects ({ActiveProjectCount})";
    public string ArchivedProjectsTabText => $"Archived Projects ({ArchivedProjectCount})";
    public int ActiveProjectCount => _allProjects.Count(p => !p.IsArchived);
    public int ArchivedProjectCount => _allProjects.Count(p => p.IsArchived);

    /// <summary>
    /// Set by ProjectsPage — callback to display the project form popup for create/edit operations.
    /// Receives the project to edit (null for create mode).
    /// Returns the form result with updated project data, or null if the user cancelled.
    /// </summary>
    public Func<Project?, Task<ProjectFormResult?>>? ShowProjectFormAsync { get; set; }

    /// <summary>
    /// Set by ProjectsPage — callback to display the project context menu popup.
    /// Returns the action the user selected ("Edit", "Archive", "Unarchive", "Delete"), or null if cancelled.
    /// </summary>
    public Func<Project, Task<string?>>? ShowProjectMenuPopupAsync { get; set; }

    public ICommand LoadProjectsCommand { get; }
    public ICommand CreateProjectCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ShowActiveProjectsCommand { get; }
    public ICommand ShowArchivedProjectsCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand SyncCommand { get; }
    public ICommand NavigateToProjectCommand { get; }
    public ICommand ClearSearchCommand { get; }

    // Handles the ⋮ tap on a project card — shows Edit / Archive / Delete sheet
    public ICommand ShowProjectMenuCommand { get; }

    public ProjectsViewModel(
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

        _syncContext = SynchronizationContext.Current;

        LoadProjectsCommand = new RelayCommand(OnLoadProjects);
        CreateProjectCommand = new RelayCommand(OnCreateProject);
        RefreshCommand = new RelayCommand(OnRefresh);
        ShowActiveProjectsCommand = new RelayCommand(() => ShowArchived = false);
        ShowArchivedProjectsCommand = new RelayCommand(() => ShowArchived = true);
        SearchCommand = new RelayCommand(OnSearch);
        SyncCommand = new RelayCommand(OnSync);
        NavigateToProjectCommand = new RelayCommand<Guid>(OnNavigateToProject);
        ClearSearchCommand = new RelayCommand(OnClearSearch);

        // Passes the full Project object so we have name + id without an extra lookup
        ShowProjectMenuCommand = new RelayCommand<Project?>(OnShowProjectMenu);

        _ = LoadProjectsAsync();
    }

    /// <summary>
    /// Loads all projects from the database and populates both tabs.
    /// </summary>
    public async Task LoadProjectsAsync()
    {
        try
        {
            IsLoading = true;

            var activeProjects = await _projectRepository.GetActiveProjectsAsync().ConfigureAwait(false);
            var archivedProjects = await _projectRepository.GetArchivedProjectsAsync().ConfigureAwait(false);

            UpdateOnUiThread(() =>
            {
                _allProjects.Clear();

                foreach (var project in activeProjects)
                {
                    _allProjects.Add(project);
                }
                foreach (var project in archivedProjects)
                {
                    _allProjects.Add(project);
                }

                FilterProjects();

            });
        }
        catch (InvalidOperationException ex)
        {
            await _dialogService.ShowAlertAsync("Load Failed", "Could not load projects.").ConfigureAwait(false);
        }
        finally
        {
            UpdateOnUiThread(() => IsLoading = false);
        }
    }

    /// <summary>
    /// Filters _allProjects into Projects based on the active tab.
    /// Also refreshes the tab count labels.
    /// </summary>
    private void FilterProjects()
    {
        Projects.Clear();

        IEnumerable<Project> filtered = _allProjects.Where(p => p.IsArchived == ShowArchived);

        // Apply keyword search — matches name OR any tag, case-insensitive
        var query = _searchText.Trim();
        if (!string.IsNullOrEmpty(query))
        {
            filtered = filtered.Where(p =>
                p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.Tags.Any(t => t.Name.Contains(query, StringComparison.OrdinalIgnoreCase)));
        }

        foreach (var project in filtered)
            Projects.Add(project);

        IsEmpty = Projects.Count == 0;

        OnPropertyChanged(nameof(ActiveProjectsTabText));
        OnPropertyChanged(nameof(ArchivedProjectsTabText));
        OnPropertyChanged(nameof(ActiveProjectCount));
        OnPropertyChanged(nameof(ArchivedProjectCount));
    }

    /// <summary>
    /// Opens the form popup to create a new project (null = create mode).
    /// </summary>
    private async Task CreateProjectAsync()
    {
        if (ShowProjectFormAsync == null)
        {
            return;
        }

        var result = await ShowProjectFormAsync(null).ConfigureAwait(false);

        if (result == null)
        {
            return;
        }

        try
        {
            var newProject = Project.CreateProject(result.Name, colorHex: result.ColorHex);

            // Apply all optional detail fields, including the new ones
            newProject.UpdateProjectDetails(
                colorHex: result.ColorHex,
                totalRows: result.TotalRows,
                notes: result.Notes,
                needleOrHookSize: result.NeedleOrHookSize);

            await _projectRepository.AddAsync(newProject).ConfigureAwait(false);
            await _projectRepository.SaveChangesAsync().ConfigureAwait(false);

            // Sync tags separately — uses ExecuteDeleteAsync + re-insert to avoid
            // change tracking issues (same strategy used for counter history)
            if (result.Tags.Count > 0)
            {
                await _projectRepository.UpdateTagsAsync(newProject.Id, result.Tags).ConfigureAwait(false);
                await _projectRepository.SaveChangesAsync().ConfigureAwait(false);
            }

            if (!string.IsNullOrWhiteSpace(result.ImagePath))
            {
                newProject.SetProjectImage(result.ImagePath);
                await _projectRepository.UpdateAsync(newProject).ConfigureAwait(false);
                await _projectRepository.SaveChangesAsync().ConfigureAwait(false);
            }

            if (result.ProjectFiles.Count > 0)
            {
                await SyncProjectFilesAsync(newProject.Id, result.ProjectFiles, _projectFileRepository)
                    .ConfigureAwait(false);
            }

            var counterNames = result.InitialCounterNames.Count > 0
                ? result.InitialCounterNames.ToList()
                : new List<string> { "Rows" };

            for (int i = 0; i < counterNames.Count; i++)
            {
                var counter = ProjectCounter.Create(newProject.Id, counterNames[i], sortOrder: i);
                await _counterRepository.AddAsync(counter).ConfigureAwait(false);
            }
            await _counterRepository.SaveChangesAsync().ConfigureAwait(false);

            await LoadProjectsAsync().ConfigureAwait(false);
            await _dialogService.ShowToastAsync($"'{result.Name}' created!").ConfigureAwait(false);
        }
        catch (InvalidOperationException ex)
        {
            await _dialogService.ShowAlertAsync("Create Failed", "Could not create project.").ConfigureAwait(false);
        }
        catch (ArgumentException ex)
        {
            await _dialogService.ShowAlertAsync("Invalid Input", ex.Message).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Opens the form popup pre-filled with the project's current data.
    /// </summary>
    private async Task EditProjectAsync(Project project)
    {
        if (ShowProjectFormAsync == null)
        {
            return;
        }

        var result = await ShowProjectFormAsync(project).ConfigureAwait(false);

        if (result == null)
        {
            return;
        }

        try
        {
            project.Rename(result.Name);
            project.UpdateProjectDetails(
                colorHex: result.ColorHex,
                totalRows: result.TotalRows,
                notes: result.Notes,
                needleOrHookSize: result.NeedleOrHookSize);

            await _projectRepository.UpdateAsync(project).ConfigureAwait(false);
            await _projectRepository.SaveChangesAsync().ConfigureAwait(false);

            // Always sync tags — UpdateTagsAsync handles the case where the list is empty
            // (it will just delete all existing tags, which is correct)
            await _projectRepository.UpdateTagsAsync(project.Id, result.Tags).ConfigureAwait(false);
            await _projectRepository.SaveChangesAsync().ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(result.ImagePath))
            {
                project.SetProjectImage(result.ImagePath);
                await _projectRepository.UpdateAsync(project).ConfigureAwait(false);
                await _projectRepository.SaveChangesAsync().ConfigureAwait(false);
            }

            await SyncProjectFilesAsync(project.Id, result.ProjectFiles, _projectFileRepository)
                    .ConfigureAwait(false);

            await LoadProjectsAsync().ConfigureAwait(false);
            await _dialogService.ShowToastAsync($"'{result.Name}' updated!").ConfigureAwait(false);
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

    /// <summary>
    /// Shows an action sheet for the tapped project card.
    /// Branches to Edit, Archive, or Delete based on what the user picks.
    /// </summary>
    private void OnShowProjectMenu(Project? project)
    {
        if (project is null)
        {
            return;
        }

        _ = ShowProjectMenuAsync(project).ContinueWith(
            t =>
            {
                if (t.IsFaulted)
            },
            TaskScheduler.FromCurrentSynchronizationContext()
        );
    }

    private async Task ShowProjectMenuAsync(Project project)
    {
        if (ShowProjectMenuPopupAsync == null)
        {
            return;
        }

        // The popup handles Archive vs Unarchive label based on project.IsArchived
        var action = await ShowProjectMenuPopupAsync(project).ConfigureAwait(true);

        // null means the user cancelled
        switch (action)
        {
            case "Edit":
                await EditProjectAsync(project).ConfigureAwait(false);
                break;

            case "Archive":
                await ArchiveProjectAsync(project.Id).ConfigureAwait(false);
                break;

            case "Unarchive":
                await UnarchiveProjectAsync(project.Id).ConfigureAwait(false);
                break;

            case "Delete":
                await DeleteProjectAsync(project.Id).ConfigureAwait(false);
                break;
        }
    }

    /// <summary>
    /// Soft delete — moves the project to the Archived tab. Reversible.
    /// </summary>
    private async Task ArchiveProjectAsync(Guid projectId)
    {
        var project = _allProjects.FirstOrDefault(p => p.Id == projectId);
        if (project == null) return;

        try
        {
            await _projectRepository.ArchiveAsync(projectId).ConfigureAwait(false);
            await _projectRepository.SaveChangesAsync().ConfigureAwait(false);

            await LoadProjectsAsync().ConfigureAwait(false);
            await _dialogService.ShowToastAsync($"'{project.Name}' archived").ConfigureAwait(false);
        }
        catch (InvalidOperationException ex)
        {
            await _dialogService.ShowAlertAsync("Archive Failed", "Could not archive project.").ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Restores an archived project back to the active tab.
    /// </summary>
    private async Task UnarchiveProjectAsync(Guid projectId)
    {
        var project = _allProjects.FirstOrDefault(p => p.Id == projectId);
        if (project == null) return;

        try
        {
            project.UnarchiveProject(); // already exists on the entity
            await _projectRepository.UpdateAsync(project).ConfigureAwait(false);
            await _projectRepository.SaveChangesAsync().ConfigureAwait(false);

            await LoadProjectsAsync().ConfigureAwait(false);
            await _dialogService.ShowToastAsync($"'{project.Name}' restored").ConfigureAwait(false);
        }
        catch (InvalidOperationException ex)
        {
            await _dialogService.ShowAlertAsync("Restore Failed", "Could not restore project.").ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Hard delete — permanently removes the project after explicit user confirmation.
    /// </summary>
    private async Task DeleteProjectAsync(Guid projectId)
    {
        var project = _allProjects.FirstOrDefault(p => p.Id == projectId);
        if (project == null) return;

        var confirmed = await ShowDeleteConfirmationAsync(project.Name).ConfigureAwait(false);
        if (!confirmed) return;

        try
        {
            await _projectRepository.DeleteAsync(projectId).ConfigureAwait(false);
            await _projectRepository.SaveChangesAsync().ConfigureAwait(false);

            UpdateOnUiThread(() =>
            {
                _allProjects.Remove(project);
                // FilterProjects() rebuilds Projects and notifies all count/tab properties
                FilterProjects();
            });

            await _dialogService.ShowToastAsync($"'{project.Name}' deleted").ConfigureAwait(false);
        }
        catch (InvalidOperationException ex)
        {
            await _dialogService.ShowAlertAsync("Delete Failed", "Could not delete project.").ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Asks the user to type DELETE to confirm a permanent action.
    /// </summary>
    private async Task<bool> ShowDeleteConfirmationAsync(string projectName)
    {
        var result = await _dialogService.ShowPromptAsync(
            title: "Delete Project?",
            message: $"This will permanently delete '{projectName}'. Type DELETE to confirm.",
            accept: "Delete",
            cancel: "Cancel",
            placeholder: "DELETE",
            maxLength: 10
        ).ConfigureAwait(false);

        return result?.Equals("DELETE", StringComparison.OrdinalIgnoreCase) == true;
    }

    private async void OnNavigateToProject(Guid projectId)
    {
        if (projectId == Guid.Empty) return;
        await _navigationService.NavigateToAsync($"SingleProjectPage?ProjectId={projectId}").ConfigureAwait(false);
    }

    // Command handlers
    private void OnLoadProjects()
    {
        _ = LoadProjectsAsync();
    }

    private void OnCreateProject()
    {
        _ = CreateProjectAsync();
    }

    private void OnRefresh()
    {
        _ = LoadProjectsAsync();
    }

    private void OnSearch()
    {
        IsSearchVisible = !IsSearchVisible;
        if (!IsSearchVisible)
        {
            // Hide → clear search and restore full list
            _searchText = string.Empty;
            OnPropertyChanged(nameof(SearchText));
            FilterProjects();
        }
    }

    private void OnClearSearch()
    {
        _searchText = string.Empty;
        OnPropertyChanged(nameof(SearchText));
        IsSearchVisible = false;
        FilterProjects();
    }

    private void OnSync()
    {
        // TODO: Implement sync functionality
    }

    /// <summary>
    /// Runs an action on the UI thread using the captured SynchronizationContext.
    /// Required when updating ObservableCollections from background tasks.
    /// </summary>
    private void UpdateOnUiThread(Action action)
    {
        if (_syncContext != null)
            _syncContext.Post(_ => action(), null);
        else
            action();
    }

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
