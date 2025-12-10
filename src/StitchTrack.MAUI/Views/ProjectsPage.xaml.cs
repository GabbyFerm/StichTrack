using StitchTrack.Application.ViewModels;

namespace StitchTrack.MAUI.Views;

public partial class ProjectsPage : ContentPage
{
    public ProjectsPage(ProjectsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        System.Diagnostics.Debug.WriteLine("✅ ProjectsPage initialized with ViewModel");
    }

    /// <summary>
    /// Handles delete button click - calls ViewModel's async delete method
    /// </summary>
    private async void OnDeleteButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Guid projectId)
        {
            var viewModel = BindingContext as ProjectsViewModel;
            if (viewModel != null)
            {
                await viewModel.DeleteProjectByIdAsync(projectId);
            }
        }
    }
}
