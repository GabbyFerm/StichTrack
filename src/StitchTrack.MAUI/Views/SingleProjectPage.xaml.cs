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
        // Grid rebuild is handled by OnViewModelPropertyChanged
        // when LoadProjectAsync fires OnPropertyChanged(string.Empty)
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
    /// Builds counter chips showing name, current count, and a bin delete button.
    /// Follows the same 2-column grid pattern as file chips.
    /// </summary>
    private void BuildCounterChips()
    {
        CountersGrid.Children.Clear();

        var counters = _viewModel.Counters;
        if (counters.Count == 0) return;

        var isDark = Microsoft.Maui.Controls.Application.Current?.RequestedTheme == AppTheme.Dark;

        for (int i = 0; i < counters.Count; i += 2)
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

            row.Add(CreateCounterChip(counters[i], isDark), column: 0, row: 0);

            if (i + 1 < counters.Count)
                row.Add(CreateCounterChip(counters[i + 1], isDark), column: 1, row: 0);
            else
                row.Add(new BoxView { IsVisible = false }, column: 1, row: 0);

            CountersGrid.Children.Add(row);
        }
    }

    /// <summary>
    /// A chip showing [counter icon] name · count  [bin icon].
    /// Bin tap triggers delete with confirmation via ViewModel.
    /// </summary>
    private Border CreateCounterChip(ProjectCounter counter, bool isDark)
    {
        var chip = new Border
        {
            StrokeThickness = 0,
            BackgroundColor = isDark
                ? Color.FromArgb("#4A5259")
                : Color.FromArgb("#F5EDD3"),
            Padding = new Thickness(10, 8),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
            {
                CornerRadius = new CornerRadius(8)
            }
        };

        var textColor = isDark ? Colors.White : Color.FromArgb("#2C3338");

        var content = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
        {
            new ColumnDefinition { Width = GridLength.Star },   // name + count
            new ColumnDefinition { Width = GridLength.Auto }    // bin icon
        },
            ColumnSpacing = 6
        };

        // Name · count
        content.Add(new Label
        {
            Text = $"{counter.Name}  ·  {counter.CurrentCount}",
            FontFamily = "MontserratMedium",
            FontSize = 12,
            TextColor = textColor,
            VerticalOptions = LayoutOptions.Center,
            LineBreakMode = LineBreakMode.TailTruncation
        }, column: 0, row: 0);

        // Bin icon
        var binIcon = new Image
        {
            WidthRequest = 14,
            HeightRequest = 14,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.End,
            Source = isDark
                ? ImageSource.FromFile("bin_dark.svg")
                : ImageSource.FromFile("bin_light.svg")
        };

        var capturedId = counter.Id;
        var binTap = new TapGestureRecognizer();
        binTap.Tapped += async (_, _) => await _viewModel.RemoveCounterAsync(capturedId);
        binIcon.GestureRecognizers.Add(binTap);

        content.Add(binIcon, column: 1, row: 0);

        chip.Content = content;
        return chip;
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
            BackgroundColor = isDark
                ? Color.FromArgb("#4A5259")
                : Color.FromArgb("#F5EDD3"),
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

    private async void OnAddCounterTapped(object sender, EventArgs e)
    {
        var name = await DisplayPromptAsync(
            "Add Counter",
            "Enter a name for the new counter:",
            accept: "Add",
            cancel: "Cancel",
            placeholder: "e.g. Stitches",
            maxLength: 50,
            keyboard: Keyboard.Create(KeyboardFlags.CapitalizeSentence));

        if (string.IsNullOrWhiteSpace(name)) return;

        await _viewModel.AddCounterAsync(name);
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
            MainThread.BeginInvokeOnMainThread(() =>
            {
                BuildFilesGrid(PatternFilesGrid, _viewModel.PatternFiles);
                BuildFilesGrid(InspirationPhotosGrid, _viewModel.InspirationPhotos);
                BuildCounterChips();
            });
        }
    }
}
