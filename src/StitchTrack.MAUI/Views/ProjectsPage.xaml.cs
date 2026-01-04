using StitchTrack.Application.ViewModels;

namespace StitchTrack.MAUI.Views;

public partial class ProjectsPage : ContentPage
{
    private readonly ProjectsViewModel _viewModel;

    public ProjectsPage(ProjectsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = _viewModel;

        System.Diagnostics.Debug.WriteLine("✅ ProjectsPage initialized with ViewModel");
    }

    /// <summary>
    /// Load projects when page appears
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        System.Diagnostics.Debug.WriteLine("📱 ProjectsPage appearing - loading projects");
        await _viewModel.LoadProjectsAsync();
    }

    /// <summary>
    /// Handles delete button click - calls ViewModel's async delete method
    /// </summary>
    private async void OnDeleteButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Guid projectId)
        {
            await _viewModel.DeleteProjectByIdAsync(projectId);
        }
    }
}
