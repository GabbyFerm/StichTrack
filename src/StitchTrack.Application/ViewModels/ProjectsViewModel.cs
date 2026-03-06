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
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    private readonly SynchronizationContext? _syncContext;
    private bool _isLoading;
    private bool _isEmpty;
    private bool _showArchived;

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
                System.Diagnostics.Debug.WriteLine($"🔄 Showing {(value ? "archived" : "active")} projects");
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
    /// Set by ProjectsPage to open the form popup.
    /// Receives the project to edit (null = create mode).
    /// Returns the filled form result, or null if cancelled.
    /// </summary>
    public Func<Project?, Task<ProjectFormResult?>>? ShowProjectFormAsync { get; set; }

    /// <summary>
    /// Set by ProjectsPage to open the project menu popup.
    /// Returns the action string ("Edit", "Archive", "Unarchive", "Delete"), or null for cancel.
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

    // Handles the ⋮ tap on a project card — shows Edit / Archive / Delete sheet
    public ICommand ShowProjectMenuCommand { get; }

    public ProjectsViewModel(
        IProjectRepository projectRepository,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
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

        // Passes the full Project object so we have name + id without an extra lookup
        ShowProjectMenuCommand = new RelayCommand<Project?>(OnShowProjectMenu);

        System.Diagnostics.Debug.WriteLine("✅ ProjectsViewModel created");

        _ = LoadProjectsAsync();
    }

    /// <summary>
    /// Loads all projects from the database and populates both tabs.
    /// </summary>
    public async Task LoadProjectsAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("📂 Loading projects...");
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

                System.Diagnostics.Debug.WriteLine($"✅ Loaded {_allProjects.Count} projects ({activeProjects.Count()} active, {archivedProjects.Count()} archived)");
            });
        }
        catch (InvalidOperationException ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error loading projects: {ex.Message}");
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

        var filtered = _allProjects.Where(p => p.IsArchived == ShowArchived);

        foreach (var project in filtered)
        {
            Projects.Add(project);
        }

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
            System.Diagnostics.Debug.WriteLine("⚠️ ShowProjectFormAsync callback not set");
            return;
        }

        var result = await ShowProjectFormAsync(null).ConfigureAwait(false);

        if (result == null)
        {
            System.Diagnostics.Debug.WriteLine("⚠️ Project creation cancelled");
            return;
        }

        try
        {
            // CreateProject sets the name and color; UpdateProjectDetails adds the rest
            var newProject = Project.CreateProject(result.Name, colorHex: result.ColorHex);
            newProject.UpdateProjectDetails(
                colorHex: result.ColorHex,
                totalRows: result.TotalRows,
                notes: result.Notes
            );

            await _projectRepository.AddAsync(newProject).ConfigureAwait(false);
            await _projectRepository.SaveChangesAsync().ConfigureAwait(false);

            System.Diagnostics.Debug.WriteLine($"✅ Project created: {newProject.Name}");

            await LoadProjectsAsync().ConfigureAwait(false);
            await _dialogService.ShowToastAsync($"'{result.Name}' created!").ConfigureAwait(false);
        }
        catch (InvalidOperationException ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error creating project: {ex.Message}");
            await _dialogService.ShowAlertAsync("Create Failed", "Could not create project.").ConfigureAwait(false);
        }
        catch (ArgumentException ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Validation error: {ex.Message}");
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
            System.Diagnostics.Debug.WriteLine("⚠️ ShowProjectFormAsync callback not set");
            return;
        }

        // Pass the existing project so the popup pre-fills the form fields
        var result = await ShowProjectFormAsync(project).ConfigureAwait(false);

        if (result == null)
        {
            System.Diagnostics.Debug.WriteLine("⚠️ Project edit cancelled");
            return;
        }

        try
        {
            // Apply domain methods — the entity controls all state changes
            project.Rename(result.Name);
            project.UpdateProjectDetails(
                colorHex: result.ColorHex,
                totalRows: result.TotalRows,
                notes: result.Notes
            );

            await _projectRepository.UpdateAsync(project).ConfigureAwait(false);
            await _projectRepository.SaveChangesAsync().ConfigureAwait(false);

            System.Diagnostics.Debug.WriteLine($"✅ Project updated: {project.Name}");

            await LoadProjectsAsync().ConfigureAwait(false);
            await _dialogService.ShowToastAsync($"'{result.Name}' updated!").ConfigureAwait(false);
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
    /// Shows an action sheet for the tapped project card.
    /// Branches to Edit, Archive, or Delete based on what the user picks.
    /// </summary>
    private void OnShowProjectMenu(Project? project)
    {
        if (project is null)
        {
            System.Diagnostics.Debug.WriteLine("⚠️ ShowProjectMenu called with null project");
            return;
        }

        _ = ShowProjectMenuAsync(project).ContinueWith(
            t =>
            {
                if (t.IsFaulted)
                    System.Diagnostics.Debug.WriteLine($"❌ ShowProjectMenu failed: {t.Exception?.GetBaseException().Message}");
            },
            TaskScheduler.FromCurrentSynchronizationContext()
        );
    }

    private async Task ShowProjectMenuAsync(Project project)
    {
        if (ShowProjectMenuPopupAsync == null)
        {
            System.Diagnostics.Debug.WriteLine("⚠️ ShowProjectMenuPopupAsync callback not set");
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
            System.Diagnostics.Debug.WriteLine($"❌ Error archiving project: {ex.Message}");
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
            System.Diagnostics.Debug.WriteLine($"❌ Error unarchiving project: {ex.Message}");
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
                Projects.Remove(project);
                IsEmpty = Projects.Count == 0;
            });

            await _dialogService.ShowToastAsync($"'{project.Name}' deleted").ConfigureAwait(false);
        }
        catch (InvalidOperationException ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error deleting project: {ex.Message}");
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
        System.Diagnostics.Debug.WriteLine("🔄 Refreshing projects...");
        _ = LoadProjectsAsync();
    }

    private void OnSearch()
    {
        System.Diagnostics.Debug.WriteLine("🔍 Search tapped");
        // TODO: Implement search functionality
    }

    private void OnSync()
    {
        System.Diagnostics.Debug.WriteLine("🔄 Sync tapped");
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
}
