using Microsoft.Maui.Layouts;
using StitchTrack.Application.ViewModels;
using StitchTrack.Domain.Entities;

namespace StitchTrack.MAUI.Views;

/// <summary>
/// Code-behind for ProjectCounterPage.
/// Owns the session timer (lifecycle + disposal). ViewModel handles all business logic and time formatting.
/// Timer tick updates ViewModel → ViewModel updates UI via binding (separation of concerns).
/// This layering keeps MAUI dependencies out of the application layer.
/// </summary>
[QueryProperty(nameof(ProjectId), "ProjectId")]
// Timer is disposed in OnDisappearing — MAUI pages use lifecycle methods
// instead of IDisposable since the framework controls their lifetime
#pragma warning disable CA1001
public partial class ProjectCounterPage : ContentPage
#pragma warning restore CA1001
{
    private readonly ProjectCounterViewModel _viewModel;
    private string _projectId = string.Empty;

    // Timer lives in the MAUI layer (UI lifecycle) — ViewModel stays framework-agnostic
    private System.Timers.Timer? _sessionTimer;
    private TimeSpan _sessionDuration;

    private bool _isHandlingBackPress;

    public ProjectCounterPage(ProjectCounterViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = _viewModel;

        // Watch for session start/stop so we can run/stop the timer
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        _viewModel.RowNotesChanged += (_, _) => BuildRowNotesGrid();

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
                await DisplayAlert("Cannot Open File", "The file path is invalid.", "OK");
            }
            catch (InvalidOperationException ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Cannot open file: {ex.Message}");
                await DisplayAlert("Cannot Open File", "Could not open the pattern file.", "OK");
            }
            catch (System.IO.IOException ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ IO error: {ex.Message}");
                await DisplayAlert("Cannot Open File", "There was an error accessing the file.", "OK");
            }
        };

        System.Diagnostics.Debug.WriteLine("✅ ProjectCounterPage initialized");
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
                System.Diagnostics.Debug.WriteLine($"📌 ProjectId set: {projectId}");
            }
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadProjectAsync();
        BuildCounterPatternFilesRow();
        RowNoteRowEntry.Text = _viewModel.CurrentCount
            .ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Always clean up the timer when leaving the page
        StopTimer();
        _sessionTimer?.Dispose();
        _sessionTimer = null;

        // Auto-save count silently when leaving — covers swipe back gesture
        _ = _viewModel.AutoSaveAsync();
    }

    protected override bool OnBackButtonPressed()
    {
        if (_viewModel.IsSessionRunning && !_isHandlingBackPress)
        {
            _isHandlingBackPress = true;
            _ = MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    var result = await DisplayAlert(
                        "Active Session",
                        "You have an active session. Save your progress before leaving?",
                        "Save & Leave",
                        "Leave Without Saving");

                    if (result)
                        await _viewModel.SaveProgressAsync();

                    await Shell.Current.GoToAsync("..");
                }
                finally
                {
                    _isHandlingBackPress = false;
                }
            });
            return true;
        }
        return base.OnBackButtonPressed();
    }

    /// <summary>
    /// Watches IsSessionRunning on the ViewModel and starts/stops the timer accordingly.
    /// Keeps timer lifecycle in the Page where it belongs.
    /// </summary>
    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProjectCounterViewModel.IsSessionRunning))
        {
            if (_viewModel.IsSessionRunning)
                StartTimer();
            else
                StopTimer();
        }

        // Keep the row number entry in sync with the current count
        if (e.PropertyName == nameof(ProjectCounterViewModel.CurrentCount))
        {
            RowNoteRowEntry.Text = _viewModel.CurrentCount
                .ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    private void StartTimer()
    {
        if (_sessionTimer == null)
        {
            _sessionTimer = new System.Timers.Timer(1000);
            _sessionTimer.Elapsed += OnTimerTick;
        }

        _sessionTimer.Start();
        System.Diagnostics.Debug.WriteLine("⏱️ Session timer started");
    }

    private void StopTimer()
    {
        _sessionTimer?.Stop();
        System.Diagnostics.Debug.WriteLine("⏹️ Session timer stopped");
    }

    /// <summary>
    /// Timer callback fires every second on a background thread.
    /// Increments _sessionDuration and updates ViewModel on the main thread.
    /// ViewModel formats the display string and notifies the UI via binding.
    /// </summary>
    private void OnTimerTick(object? sender, System.Timers.ElapsedEventArgs e)
    {
        _sessionDuration = _sessionDuration.Add(TimeSpan.FromSeconds(1));

        // ViewModel update must happen on the main thread
        MainThread.BeginInvokeOnMainThread(() =>
            _viewModel.UpdateSessionTimer(_sessionDuration)
        );
    }

    /// <summary>
    /// Fired when the user taps + or presses Enter on the note text entry.
    /// Validates inputs, calls the ViewModel, then resets for the next entry.
    /// </summary>
    private async void OnAddRowNoteTapped(object sender, EventArgs e)
    {
        var rowText = RowNoteRowEntry.Text?.Trim();
        var noteText = RowNoteTextEntry.Text?.Trim();

        // Silently ignore if either field is missing or row is not a valid number
        if (string.IsNullOrWhiteSpace(rowText)
            || !int.TryParse(rowText, out var rowNumber)
            || rowNumber < 0
            || string.IsNullOrWhiteSpace(noteText))
        {
            return;
        }

        await _viewModel.AddRowNoteAsync(rowNumber, noteText);

        // Clear text, reset row number to current count, keep focus on text entry
        RowNoteTextEntry.Text = string.Empty;
        RowNoteRowEntry.Text = _viewModel.CurrentCount
            .ToString(System.Globalization.CultureInfo.InvariantCulture);
        RowNoteTextEntry.Focus();
    }

    /// <summary>
    /// Rebuilds the 2-column notes grid from the ViewModel's current RowNotes list.
    /// Each row displays up to 2 note chips; odd counts get a placeholder in the second column.
    /// Called on initial load (OnAppearing) and after every add/delete via the RowNotesChanged event.
    /// </summary>
    private void BuildRowNotesGrid()
    {
        RowNotesContainer.Children.Clear();

        var notes = _viewModel.RowNotes;
        if (notes.Count == 0) return;

        // Pair notes into rows of 2
        for (int i = 0; i < notes.Count; i += 2)
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

            row.Add(CreateNoteChip(notes[i]), column: 0, row: 0);

            // Second column — empty placeholder keeps the grid balanced for odd counts
            if (i + 1 < notes.Count)
                row.Add(CreateNoteChip(notes[i + 1]), column: 1, row: 0);
            else
                row.Add(new BoxView { IsVisible = false }, column: 1, row: 0);

            RowNotesContainer.Children.Add(row);
        }
    }

    /// <summary>
    /// Creates a single note chip with row number, note text, and a delete × button.
    /// Background adapts to light/dark mode.
    /// </summary>
    private Border CreateNoteChip(RowNote note)
    {
        var isDark = Microsoft.Maui.Controls.Application.Current?.RequestedTheme == AppTheme.Dark;

        var chip = new Border
        {
            StrokeThickness = 0,
            // Slightly darker than the card background in both modes
            BackgroundColor = isDark
                ? Color.FromArgb("#3D4449")
                : Color.FromArgb("#EBE3C8"),
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
            new ColumnDefinition { Width = GridLength.Auto }, // "R12"
            new ColumnDefinition { Width = GridLength.Star }, // note text
            new ColumnDefinition { Width = GridLength.Auto }  // ×
        },
            ColumnSpacing = 6
        };

        // Row number — gold, compact ("R12")
        content.Add(new Label
        {
            Text = $"R{note.RowNumber}",
            FontFamily = "MontserratBold",
            FontSize = 12,
            TextColor = Color.FromArgb("#E1AD37"), // BrandGold
            VerticalOptions = LayoutOptions.Center
        }, column: 0, row: 0);

        // Note text — truncates if too long for the chip
        content.Add(new Label
        {
            Text = note.NoteText,
            FontFamily = "MontserratRegular",
            FontSize = 12,
            TextColor = textColor,
            VerticalOptions = LayoutOptions.Center,
            LineBreakMode = LineBreakMode.TailTruncation
        }, column: 1, row: 0);

        // Delete button — subtle opacity so it doesn't compete with the note text
        var deleteLabel = new Label
        {
            Text = "×",
            FontFamily = "MontserratBold",
            FontSize = 16,
            TextColor = textColor,
            Opacity = 0.5,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.End
        };

        var capturedId = note.Id; // capture for the async closure
        var deleteTap = new TapGestureRecognizer();
        deleteTap.Tapped += async (_, _) => await _viewModel.DeleteRowNoteAsync(capturedId);
        deleteLabel.GestureRecognizers.Add(deleteTap);

        content.Add(deleteLabel, column: 2, row: 0);

        chip.Content = content;
        return chip;
    }

    /// <summary>
    /// Builds the project files row with a gold left accent bar, file icon, "Pattern:" label,
    /// and inline tappable filenames separated by |.
    /// Called on page appear to populate the FlexLayout with current pattern files from ViewModel.
    /// Supports multiple files and shows each filename as a tappable link.
    /// </summary>
    private void BuildCounterPatternFilesRow()
    {
        // Remove previously added file links — keep XAML children (icon + "Pattern:" label)
        while (CounterPatternFilesGrid.Children.Count > 2)
            CounterPatternFilesGrid.Children.RemoveAt(2);

        var files = _viewModel.PatternFiles;
        if (files.Count == 0) return;

        var isDark = Microsoft.Maui.Controls.Application.Current?.RequestedTheme == AppTheme.Dark;
        var secondaryColor = isDark
            ? Colors.White.WithAlpha(0.5f)
            : Color.FromArgb("#6B7280");

        for (int i = 0; i < files.Count; i++)
        {
            var file = files[i];

            var fileLabel = new Label
            {
                Text = file.FileName,
                FontFamily = "MontserratMedium",
                FontSize = 13,
                TextColor = Color.FromArgb("#E1AD37"),
                VerticalOptions = LayoutOptions.Center
            };

            var capturedFile = file;
            var tap = new TapGestureRecognizer();
            tap.Tapped += async (_, _) =>
            {
                if (_viewModel.OpenFileAsync != null && !string.IsNullOrWhiteSpace(capturedFile.FilePath))
                    await _viewModel.OpenFileAsync(capturedFile.FilePath);
            };
            fileLabel.GestureRecognizers.Add(tap);
            CounterPatternFilesGrid.Children.Add(fileLabel);

            if (i < files.Count - 1)
            {
                CounterPatternFilesGrid.Children.Add(new Label
                {
                    Text = "  |  ",
                    FontSize = 13,
                    TextColor = secondaryColor,
                    VerticalOptions = LayoutOptions.Center
                });
            }
        }
    }

    private Border CreateCounterFileChip(ProjectFile file)
    {
        var isDark = Microsoft.Maui.Controls.Application.Current?.RequestedTheme == AppTheme.Dark;
        var isPhoto = file.ContentType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == true;

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
            TextColor = Color.FromArgb("#E1AD37"),
            VerticalOptions = LayoutOptions.Center,
            LineBreakMode = LineBreakMode.TailTruncation
        });

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) =>
        {
            if (_viewModel.OpenFileAsync != null && !string.IsNullOrWhiteSpace(file.FilePath))
                await _viewModel.OpenFileAsync(file.FilePath);
        };
        chip.GestureRecognizers.Add(tap);

        chip.Content = content;
        return chip;
    }
}
