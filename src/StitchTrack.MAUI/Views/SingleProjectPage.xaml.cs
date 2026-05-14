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

        _viewModel.ShowProjectFormAsync = ShowProjectFormPopupAsync;

        // Rebuild dynamic file grids when the project reloads after an edit
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;

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
                await DisplayAlert("Cannot Open File", "Could not open the file due to an invalid operation.", "OK");
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
        BuildFilesGrid(PatternFilesGrid, _viewModel.PatternFiles);
        BuildFilesGrid(InspirationPhotosGrid, _viewModel.InspirationPhotos);
    }

    /// <summary>
    /// Builds a 2-column grid of file chips for either pattern files or inspiration photos.
    /// Used to display project attachments with proper layout for odd/even counts.
    /// </summary>
    private void BuildFilesGrid(VerticalStackLayout container, IReadOnlyList<ProjectFile> files)
    {
        container.Children.Clear();
        if (files.Count == 0) return;

        for (int i = 0; i < files.Count; i += 2)
        {
            var row = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star }
            },
                ColumnSpacing = 8
            };

            row.Add(CreatePageFileChip(files[i]), column: 0, row: 0);

            if (i + 1 < files.Count)
                row.Add(CreatePageFileChip(files[i + 1]), column: 1, row: 0);
            else
                row.Add(new BoxView { IsVisible = false }, column: 1, row: 0);

            container.Children.Add(row);
        }
    }

    /// <summary>
    /// Creates a single file chip with icon (📄 for PDF, 🖼️ for image) and tappable filename.
    /// Used for both pattern files and inspiration photos.
    /// </summary>
    private Border CreatePageFileChip(ProjectFile file)
    {
        var isDark = Microsoft.Maui.Controls.Application.Current?.RequestedTheme == AppTheme.Dark;

        var chip = new Border
        {
            StrokeThickness = 0,
            BackgroundColor = isDark ? Color.FromArgb("#3D4449") : Color.FromArgb("#EBE3C8"),
            Padding = new Thickness(10, 8),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
            {
                CornerRadius = new CornerRadius(8)
            }
        };

        var isPhoto = file.ContentType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == true;

        var content = new HorizontalStackLayout { Spacing = 6 };

        content.Children.Add(new Label
        {
            Text = isPhoto ? "🖼️" : "📄",
            FontSize = 12,
            VerticalOptions = LayoutOptions.Center
        });

        content.Children.Add(new Label
        {
            Text = file.FileName,
            FontFamily = "MontserratMedium",
            FontSize = 12,
            TextColor = Color.FromArgb("#E1AD37"), // BrandGold
            VerticalOptions = LayoutOptions.Center,
            LineBreakMode = LineBreakMode.TailTruncation
        });

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) => await _viewModel.OpenProjectFileAsync(file.FilePath);
        chip.GestureRecognizers.Add(tap);

        chip.Content = content;
        return chip;
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

    /// <summary>
    /// Rebuilds dynamic file grids when the ViewModel signals a full property refresh.
    /// OnPropertyChanged(string.Empty) fires at the end of LoadProjectAsync —
    /// this covers the case where edit closes a popup without triggering OnAppearing.
    /// </summary>
    private void OnViewModelPropertyChanged(object? sender,
        System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName))
        {
            BuildFilesGrid(PatternFilesGrid, _viewModel.PatternFiles);
            BuildFilesGrid(InspirationPhotosGrid, _viewModel.InspirationPhotos);
        }
    }
}
