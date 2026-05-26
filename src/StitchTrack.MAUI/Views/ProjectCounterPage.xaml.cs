using StitchTrack.Application.ViewModels;
using StitchTrack.Domain.Entities;
using System.Globalization;

namespace StitchTrack.MAUI.Views;

/// <summary>
/// Code-behind for ProjectCounterPage.
/// Owns the session timer (lifecycle + disposal).
/// Builds counter cards dynamically from _viewModel.Counters via BuildCounterCards().
/// Each card wires directly to per-counter ViewModel methods by counter ID.
/// </summary>
[QueryProperty(nameof(ProjectId), "ProjectId")]
#pragma warning disable CA1001  // Timer disposed in OnDisappearing via MAUI lifecycle
public partial class ProjectCounterPage : ContentPage
#pragma warning restore CA1001
{
    private readonly ProjectCounterViewModel _viewModel;
    private string _projectId = string.Empty;

    private System.Timers.Timer? _sessionTimer;
    private TimeSpan _sessionDuration;
    private bool _isHandlingBackPress;

    // Maps counter ID → its count Label so we can update just the text without a full rebuild
    private readonly Dictionary<Guid, Label> _counterDisplayLabels = new();

    public ProjectCounterPage(ProjectCounterViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = _viewModel;

        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        _viewModel.RowNotesChanged += (_, _) => BuildRowNotesGrid();

        // CountersChanged fires after LoadProjectAsync and after add/delete/count changes
        _viewModel.CountersChanged += OnCountersChanged;

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
                await DisplayAlert("Cannot Open File", "Could not open the file.", "OK");
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
        // BuildCounterCards() and BuildRowNotesGrid() are triggered by
        // CountersChanged and RowNotesChanged events fired from LoadProjectAsync
        BuildCounterPatternFilesRow();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopTimer();
        _sessionTimer?.Dispose();
        _sessionTimer = null;
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

    // ─── Counter cards ────────────────────────────────────────────

    /// <summary>
    /// Handles CountersChanged from the ViewModel.
    /// Full rebuild when counter count changes (add/delete); label-only update otherwise.
    /// </summary>
    private void OnCountersChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_counterDisplayLabels.Count != _viewModel.Counters.Count)
            {
                // Structure changed — full rebuild
                BuildCounterCards();
            }
            else
            {
                // Only counts changed — update labels in place (no layout thrash)
                foreach (var counter in _viewModel.Counters)
                {
                    if (_counterDisplayLabels.TryGetValue(counter.Id, out var label))
                        label.Text = counter.CurrentCount.ToString(CultureInfo.InvariantCulture);
                }
            }

            // Keep row note entry in sync with primary counter
            RowNoteRowEntry.Text = _viewModel.CurrentCount
                .ToString(CultureInfo.InvariantCulture);
        });
    }

    /// <summary>
    /// Clears and rebuilds all counter cards from the ViewModel's Counters list.
    /// Called on initial load and when counters are added or deleted.
    /// </summary>
    private void BuildCounterCards()
    {
        CountersContainer.Children.Clear();
        _counterDisplayLabels.Clear();

        foreach (var counter in _viewModel.Counters)
            CountersContainer.Children.Add(CreateCounterCard(counter));

        System.Diagnostics.Debug.WriteLine($"🔢 Built {_viewModel.Counters.Count} counter card(s)");
    }

    /// <summary>
    /// Builds a single counter card with name, [−] count [+], and Undo/Reset actions.
    /// Wires tap handlers directly to ViewModel methods via the counter's ID.
    /// </summary>
    private Border CreateCounterCard(ProjectCounter counter)
    {
        var isDark = Microsoft.Maui.Controls.Application.Current?.RequestedTheme == AppTheme.Dark;
        var capturedId = counter.Id;

        // Count label — tracked so OnCountersChanged can update it without a full rebuild
        var countLabel = new Label
        {
            Text = counter.CurrentCount.ToString(CultureInfo.InvariantCulture),
            FontFamily = "MontserratExtraBold",
            FontSize = 70,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            TextColor = isDark ? Colors.White : Color.FromArgb("#2C3338")
        };
        _counterDisplayLabels[counter.Id] = countLabel;

        // [−] decrease button
        var decrementBorder = CreateCounterButton(
            "minus.svg",
            isDark ? Color.FromArgb("#5E6B76") : Color.FromArgb("#424B54"));
        var decrementTap = new TapGestureRecognizer();
        decrementTap.Tapped += (_, _) => _viewModel.DecrementCounter(capturedId);
        decrementBorder.GestureRecognizers.Add(decrementTap);

        // [+] increase button
        var incrementBorder = CreateCounterButton(
            "plus.svg",
            Color.FromArgb("#E1AD37"));
        var incrementTap = new TapGestureRecognizer();
        incrementTap.Tapped += (_, _) => _viewModel.IncrementCounter(capturedId);
        incrementBorder.GestureRecognizers.Add(incrementTap);

        // [−] count [+] row
        var counterRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
        {
            new ColumnDefinition { Width = GridLength.Auto },
            new ColumnDefinition { Width = GridLength.Star },
            new ColumnDefinition { Width = GridLength.Auto }
        },
            ColumnSpacing = 16
        };
        counterRow.Add(decrementBorder, 0, 0);
        counterRow.Add(countLabel, 1, 0);
        counterRow.Add(incrementBorder, 2, 0);

        // ─── Header row: [icon + name]  ...  [reset icon + Reset] ────

        var counterIcon = new Image
        {
            WidthRequest = 16,
            HeightRequest = 16,
            VerticalOptions = LayoutOptions.Center,
            Source = isDark
                ? ImageSource.FromFile("counter_dark.svg")
                : ImageSource.FromFile("counter_light.svg")
        };

        var namePart = new HorizontalStackLayout
        {
            Spacing = 8,
            VerticalOptions = LayoutOptions.Center,
            Children =
        {
            counterIcon,
            new Label
            {
                Text = counter.Name,
                FontFamily = "MontserratSemiBold",
                FontSize = 14,
                VerticalOptions = LayoutOptions.Center,
                TextColor = isDark ? Colors.White : Color.FromArgb("#2C3338")
            }
        }
        };

        var resetIcon = new Image
        {
            WidthRequest = 14,
            HeightRequest = 14,
            VerticalOptions = LayoutOptions.Center,
            Source = isDark
                ? ImageSource.FromFile("reset_dark.svg")
                : ImageSource.FromFile("reset_light.svg")
        };

        var resetPart = new HorizontalStackLayout
        {
            Spacing = 6,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center,
            Children =
        {
            resetIcon,
            new Label
            {
                Text = "Reset",
                FontFamily = "MontserratBold",
                FontSize = 12,
                VerticalOptions = LayoutOptions.Center,
                TextColor = isDark
                    ? Colors.White.WithAlpha(0.6f)
                    : Color.FromArgb("#6B7280")
            }
        }
        };

        var resetTap = new TapGestureRecognizer();
        resetTap.Tapped += async (_, _) => await _viewModel.ResetCounterAsync(capturedId);
        resetPart.GestureRecognizers.Add(resetTap);

        var headerRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
        {
            new ColumnDefinition { Width = GridLength.Star },
            new ColumnDefinition { Width = GridLength.Auto }
        }
        };
        headerRow.Add(namePart, 0, 0);
        headerRow.Add(resetPart, 1, 0);

        // ─── Card ─────────────────────────────────────────────────────

        return new Border
        {
            Padding = new Thickness(16, 12, 16, 14),
            StrokeThickness = 0,
            BackgroundColor = isDark
                ? Color.FromArgb("#4A5259")
                : Color.FromArgb("#F5EDD3"),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
            {
                CornerRadius = new CornerRadius(12)
            },
            Content = new VerticalStackLayout
            {
                Spacing = 10,
                Children = { headerRow, counterRow }  // no actionRow — undo removed, reset in header
            }
        };
    }

    // Creates a square counter button (− or +) with an icon
    private static Border CreateCounterButton(string iconSource, Color backgroundColor)
    {
        var image = new Image
        {
            Source = iconSource,
            HeightRequest = 28,
            WidthRequest = 28,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        // Force white tint — same as toolkit:IconTintColorBehavior TintColor="White" in XAML
        image.Behaviors.Add(new CommunityToolkit.Maui.Behaviors.IconTintColorBehavior
        {
            TintColor = Colors.White
        });

        return new Border
        {
            WidthRequest = 62,
            HeightRequest = 62,
            StrokeThickness = 0,
            BackgroundColor = backgroundColor,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
            {
                CornerRadius = new CornerRadius(8)
            },
            Content = image
        };
    }   

    // ─── Add counter ──────────────────────────────────────────────

    /// <summary>
    /// Tapped on "+ Add Counter" — prompts for a name and adds via ViewModel.
    /// </summary>
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

    // ─── Session timer ────────────────────────────────────────────

    private void OnViewModelPropertyChanged(
        object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProjectCounterViewModel.IsSessionRunning))
        {
            if (_viewModel.IsSessionRunning) StartTimer(); else StopTimer();
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

    private void OnTimerTick(object? sender, System.Timers.ElapsedEventArgs e)
    {
        _sessionDuration = _sessionDuration.Add(TimeSpan.FromSeconds(1));
        MainThread.BeginInvokeOnMainThread(() =>
            _viewModel.UpdateSessionTimer(_sessionDuration));
    }

    // ─── Row notes ────────────────────────────────────────────────

    private async void OnAddRowNoteTapped(object sender, EventArgs e)
    {
        var rowText = RowNoteRowEntry.Text?.Trim();
        var noteText = RowNoteTextEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(rowText)
            || !int.TryParse(rowText, out var rowNumber)
            || rowNumber < 0
            || string.IsNullOrWhiteSpace(noteText))
            return;

        await _viewModel.AddRowNoteAsync(rowNumber, noteText);

        RowNoteTextEntry.Text = string.Empty;
        RowNoteRowEntry.Text = _viewModel.CurrentCount
            .ToString(CultureInfo.InvariantCulture);
        RowNoteTextEntry.Focus();
    }

    private void BuildRowNotesGrid()
    {
        RowNotesContainer.Children.Clear();

        var notes = _viewModel.RowNotes;
        if (notes.Count == 0) return;

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

            if (i + 1 < notes.Count)
                row.Add(CreateNoteChip(notes[i + 1]), column: 1, row: 0);
            else
                row.Add(new BoxView { IsVisible = false }, column: 1, row: 0);

            RowNotesContainer.Children.Add(row);
        }
    }

    private Border CreateNoteChip(RowNote note)
    {
        var isDark = Microsoft.Maui.Controls.Application.Current?.RequestedTheme == AppTheme.Dark;

        var chip = new Border
        {
            StrokeThickness = 0,
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
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = 6
        };

        content.Add(new Label
        {
            Text = $"R{note.RowNumber}",
            FontFamily = "MontserratBold",
            FontSize = 12,
            TextColor = Color.FromArgb("#E1AD37"),
            VerticalOptions = LayoutOptions.Center
        }, column: 0, row: 0);

        content.Add(new Label
        {
            Text = note.NoteText,
            FontFamily = "MontserratRegular",
            FontSize = 12,
            TextColor = textColor,
            VerticalOptions = LayoutOptions.Center,
            LineBreakMode = LineBreakMode.TailTruncation
        }, column: 1, row: 0);

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

        var capturedId = note.Id;
        var deleteTap = new TapGestureRecognizer();
        deleteTap.Tapped += async (_, _) => await _viewModel.DeleteRowNoteAsync(capturedId);
        deleteLabel.GestureRecognizers.Add(deleteTap);
        content.Add(deleteLabel, column: 2, row: 0);

        chip.Content = content;
        return chip;
    }

    // ─── Pattern files ────────────────────────────────────────────

    private void BuildCounterPatternFilesRow()
    {
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
