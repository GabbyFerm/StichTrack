using CommunityToolkit.Maui.Views;
using StitchTrack.Application.Models;
using StitchTrack.Application.ViewModels;
using StitchTrack.Domain.Entities;
using StitchTrack.MAUI.Controls;

namespace StitchTrack.MAUI.Views;

[QueryProperty(nameof(ProjectId), "ProjectId")]
public partial class SingleProjectPage : ContentPage
{
    private readonly SingleProjectViewModel _viewModel;
    private string _projectId = string.Empty;

    public SingleProjectPage(SingleProjectViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = _viewModel;

        // Wire popup callback — same pattern as ProjectsPage
        _viewModel.ShowProjectFormAsync = ShowProjectFormPopupAsync;

        _viewModel.OpenFileAsync = async (filePath) =>
        {
            try
            {
                await Launcher.Default.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(filePath)
                });
            }
            catch (ArgumentException ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Invalid file path: {ex.Message}");
                await DisplayAlert("Cannot Open File", "The specified file path is invalid.", "OK");
            }
            catch (InvalidOperationException ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Invalid operation: {ex.Message}");
                await DisplayAlert("Cannot Open File", "Could not open the pattern file due to an invalid operation.", "OK");
            }
            catch (System.IO.IOException ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ IO error: {ex.Message}");
                await DisplayAlert("Cannot Open File", "There was an error accessing the file.", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Unexpected error opening file: {ex.Message}");
                await DisplayAlert("Cannot Open File", "Could not open the pattern file.", "OK");
                throw;
            }
        };

        System.Diagnostics.Debug.WriteLine("✅ SingleProjectPage initialized");
    }

    public string ProjectId
    {
        get => _projectId;
        set
        {
            _projectId = value;
            if (Guid.TryParse(value, out var projectId))
            {
                _viewModel.ProjectId = projectId;
                System.Diagnostics.Debug.WriteLine($"📌 ProjectId set:  {projectId}");
            }
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadProjectAsync();
    }

    /// <summary>
    /// Opens the ProjectFormPopup in edit mode pre-filled with the project data.
    /// Returns the form result, or null if the user cancelled.
    /// </summary>
    private async Task<ProjectFormResult?> ShowProjectFormPopupAsync(Project? project)
    {
        ProjectFormResult? formResult = null;

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var popup = new ProjectFormPopup(project);
            var result = await this.ShowPopupAsync(popup);
            formResult = result as ProjectFormResult;
        });

        return formResult;
    }
}
