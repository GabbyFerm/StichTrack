using StitchTrack.Application.ViewModels;

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
}
