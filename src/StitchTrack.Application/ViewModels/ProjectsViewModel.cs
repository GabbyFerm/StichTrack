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

    // Observable collection of ALL projects
    private readonly ObservableCollection<Project> _allProjects = new();

    // Filtered projects based on ShowArchived flag
    public ObservableCollection<Project> Projects { get; } = new();

    // Whether to show archived or active projects
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

    // Loading state
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

    // Empty state (show "no projects" message)
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
    public string ActiveProjectsTabText => $"Active projects ({ActiveProjectCount})";
    public string ArchivedProjectsTabText => $"Archived Projects ({ArchivedProjectCount})";
    public int ActiveProjectCount => _allProjects.Count(p => !p.IsArchived);
    public int ArchivedProjectCount => _allProjects.Count(p => p.IsArchived);

    // Commands
    public ICommand LoadProjectsCommand { get; }
    public ICommand CreateProjectCommand { get; }
    public ICommand DeleteProjectCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ShowActiveProjectsCommand { get; }
    public ICommand ShowArchivedProjectsCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand SyncCommand { get; }
    public ICommand NavigateToProjectCommand { get; }

    public ProjectsViewModel(
        IProjectRepository projectRepository,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

        _syncContext = SynchronizationContext.Current;

        // Initialize commands
        LoadProjectsCommand = new RelayCommand(OnLoadProjects);
        CreateProjectCommand = new RelayCommand(OnCreateProject);
        DeleteProjectCommand = new RelayCommand(OnDeleteProject);
        RefreshCommand = new RelayCommand(OnRefresh);
        ShowActiveProjectsCommand = new RelayCommand(() => ShowArchived = false);
        ShowArchivedProjectsCommand = new RelayCommand(() => ShowArchived = true);
        SearchCommand = new RelayCommand(OnSearch);
        SyncCommand = new RelayCommand(OnSync);
        NavigateToProjectCommand = new RelayCommand<Guid>(OnNavigateToProject);

        System.Diagnostics.Debug.WriteLine("✅ ProjectsViewModel created");

        // Load projects on initialization
        _ = LoadProjectsAsync();
    }

    /// <summary>
    /// Loads all projects (both active and archived) from the database.
    /// </summary>
    public async Task LoadProjectsAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("📂 Loading projects...");
            IsLoading = true;

            // ✅ UPDATED: Load both active and archived
            var activeProjects = await _projectRepository.GetActiveProjectsAsync().ConfigureAwait(false);
            var archivedProjects = await _projectRepository.GetArchivedProjectsAsync().ConfigureAwait(false);

            UpdateOnUiThread(() =>
            {
                _allProjects.Clear();

                // Add all projects to internal collection
                foreach (var project in activeProjects)
                {
                    _allProjects.Add(project);
                }
                foreach (var project in archivedProjects)
                {
                    _allProjects.Add(project);
                }

                // Filter based on current tab
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
    /// Filters projects based on ShowArchived flag.
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

        // Notify counts changed
        OnPropertyChanged(nameof(ActiveProjectsTabText));
        OnPropertyChanged(nameof(ArchivedProjectsTabText));
        OnPropertyChanged(nameof(ActiveProjectCount));
        OnPropertyChanged(nameof(ArchivedProjectCount));
    }

    /// <summary>
    /// Creates a new project with user-provided name.
    /// </summary>
    private async Task CreateProjectAsync()
    {
        try
        {
            var projectName = await _dialogService.ShowPromptAsync(
                title: "New Project",
                message: "Enter project name:",
                accept: "Create",
                cancel: "Cancel",
                placeholder: "My Knitting Project",
                maxLength: 200
            ).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(projectName))
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Project creation cancelled");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"➕ Creating project: {projectName}");

            var newProject = Project.CreateProject(projectName.Trim());

            await _projectRepository.AddAsync(newProject).ConfigureAwait(false);
            await _projectRepository.SaveChangesAsync().ConfigureAwait(false);

            System.Diagnostics.Debug.WriteLine($"✅ Project created: {newProject.Id}");

            // Reload projects to show new one
            await LoadProjectsAsync().ConfigureAwait(false);

            await _dialogService.ShowToastAsync($"Project '{projectName}' created! ").ConfigureAwait(false);
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
    /// Navigation to single project page.
    /// </summary>
    private async void OnNavigateToProject(Guid projectId)
    {
        if (projectId == Guid.Empty) 
        {
            System.Diagnostics.Debug.WriteLine("⚠️ Invalid project ID");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"🔗 Navigating to project: {projectId}");

        await _navigationService.NavigateToAsync($"SingleProjectPage?ProjectId={projectId}").ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes a project (soft delete - sets IsArchived = true).
    /// Shows confirmation dialog before deleting.
    /// </summary>
    private async Task DeleteProjectAsync(Guid projectId)
    {
        try
        {
            var project = _allProjects.FirstOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Project not found: {projectId}");
                return;
            }

            // Show confirmation dialog
            var confirmed = await ShowDeleteConfirmationAsync(project.Name).ConfigureAwait(false);
            if (!confirmed)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Delete cancelled by user: {project.Name}");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"🗑️ Archiving project: {project.Name}");

            await _projectRepository.DeleteAsync(projectId).ConfigureAwait(false);
            await _projectRepository.SaveChangesAsync().ConfigureAwait(false);

            System.Diagnostics.Debug.WriteLine($"✅ Project archived: {project.Name}");

            // Remove from both collections
            UpdateOnUiThread(() =>
            {
                _allProjects.Remove(project);
                Projects.Remove(project);
                IsEmpty = Projects.Count == 0;
            });

            await _dialogService.ShowToastAsync($"Project '{project.Name}' deleted").ConfigureAwait(false);
        }
        catch (InvalidOperationException ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error deleting project: {ex.Message}");
            await _dialogService.ShowAlertAsync("Delete Failed", "Could not delete project.").ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Shows confirmation dialog for deleting a project.
    /// </summary>
    private async Task<bool> ShowDeleteConfirmationAsync(string projectName)
    {
        // For now, use DisplayPromptAsync to get yes/no
        // In future, implement proper DisplayActionSheet
        var result = await _dialogService.ShowPromptAsync(
            title: "Delete Project?",
            message: $"Are you sure you want to delete '{projectName}'? Type 'DELETE' to confirm.",
            accept: "Delete",
            cancel: "Cancel",
            placeholder: "DELETE",
            maxLength: 10
        ).ConfigureAwait(false);

        return result?.Equals("DELETE", StringComparison.OrdinalIgnoreCase) == true;
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

    private void OnDeleteProject()
    {
        // This will be called with project ID as parameter from UI
        System.Diagnostics.Debug.WriteLine("⚠️ DeleteProject called without parameter - use DeleteProjectAsync(Guid)");
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
    /// Public method to delete a specific project (called from UI).
    /// </summary>
    public async Task DeleteProjectByIdAsync(Guid projectId)
    {
        await DeleteProjectAsync(projectId).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes an action on the UI thread.
    /// </summary>
    private void UpdateOnUiThread(Action action)
    {
        if (_syncContext != null)
        {
            _syncContext.Post(_ => action(), null);
        }
        else
        {
            action();
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
