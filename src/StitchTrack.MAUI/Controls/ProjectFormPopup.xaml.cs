using CommunityToolkit.Maui.Views;
using StitchTrack.Application.Models;
using StitchTrack.Domain.Entities;

namespace StitchTrack.MAUI.Controls;

/// <summary>
/// Reusable popup for creating or editing a project.
/// Handles photo upload (camera/library) and file attachment support (PDF patterns, inspiration photos).
/// Multiple files of different types are now supported (replaced single pattern upload).
/// Returns a ProjectFormResult on save, or null on cancel.
/// </summary>
public partial class ProjectFormPopup : Popup
{
    private string _selectedColorHex;
    private string? _selectedImagePath;
    private readonly List<PendingProjectFile> _pendingFiles = new();
    private readonly bool _isEditMode;

    private readonly List<string> _selectedTags = new();
    private readonly List<string> _selectedCounterNames = new();

    public ProjectFormPopup(Project? existingProject = null)
    {
        InitializeComponent();

        _isEditMode = existingProject != null;

        FormTitleLabel.Text = _isEditMode ? "Edit Project" : "New Project";
        SaveButtonLabel.Text = _isEditMode ? "SAVE" : "CREATE";

        _selectedColorHex = existingProject?.ColorHex ?? ProjectColors.GetRandomColor();
        _selectedImagePath = existingProject?.ImagePath;

        BuildColorPicker();

        if (_isEditMode && existingProject != null)
        {
            NameEntry.Text = existingProject.Name;
            TotalRowsEntry.Text = existingProject.TotalRows?.ToString(
                System.Globalization.CultureInfo.InvariantCulture);
            NotesEditor.Text = existingProject.Notes;

            if (!string.IsNullOrWhiteSpace(existingProject.ImagePath))
            {
                PhotoFileNameLabel.Text = "Cover photo set ✓";
                PhotoFileNameLabel.IsVisible = true;
                PhotoLabel.Text = "Change Cover Photo";
            }

            foreach (var file in existingProject.ProjectFiles.OrderByDescending(f => f.UploadedAt))
            {
                _pendingFiles.Add(new PendingProjectFile(
                    ExistingId: file.Id,
                    FileName: file.FileName,
                    FilePath: file.FilePath,
                    FileSizeBytes: 0,
                    ContentType: file.ContentType ?? string.Empty,
                    FileType: file.FileType));
            }

            BuildFileChips(PatternFilesContainer, ProjectFileType.Pattern);
            BuildFileChips(InspirationPhotosContainer, ProjectFileType.InspirationPhoto);

            // Pre-fill needle/hook size
            NeedleSizeEntry.Text = existingProject.NeedleOrHookSize;

            // Pre-fill tags — project must be loaded with .Include(p => p.Tags)
            foreach (var tag in existingProject.Tags.OrderBy(t => t.ColorIndex))
                _selectedTags.Add(tag.Name);
        }

        BuildTagChips();

        if (!_isEditMode)
            BuildCounterChips();   // counters only shown in create mode
        else
            CounterSection.IsVisible = false;  // hide in edit mode — managed from SingleProjectPage

    }

    // Static to avoid allocating new arrays on every file picker call
    private static readonly FilePickerFileType PdfFileType = new(
        new Dictionary<DevicePlatform, IEnumerable<string>>
        {
        { DevicePlatform.Android, new[] { "application/pdf" } },
        { DevicePlatform.iOS,     new[] { "com.adobe.pdf" } },
        { DevicePlatform.WinUI,   new[] { ".pdf" } }
        });

    // ─── Color picker ────────────────────────────────────────────

    private void BuildColorPicker()
    {
        ColorPickerContainer.Children.Clear();

        foreach (var hex in ProjectColors.Palette)
        {
            var ring = new Border
            {
                WidthRequest = 32,
                HeightRequest = 32,
                StrokeThickness = _selectedColorHex == hex ? 2 : 0,
                Stroke = Color.FromArgb("#F59E0B"),
                BackgroundColor = Colors.Transparent,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.Ellipse()
            };

            var dot = new Border
            {
                WidthRequest = 24,
                HeightRequest = 24,
                StrokeThickness = 0,
                BackgroundColor = Color.FromArgb(hex),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.Ellipse()
            };

            ring.Content = dot;

            var capturedHex = hex;
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => OnColorSelected(capturedHex);
            ring.GestureRecognizers.Add(tap);

            ColorPickerContainer.Children.Add(ring);
        }
    }

    private void OnColorSelected(string hex)
    {
        _selectedColorHex = hex;
        BuildColorPicker();
    }

    /// <summary>
    /// Called when the user taps the "+" button next to the tag entry.
    /// </summary>
    private void OnAddTagTapped(object sender, TappedEventArgs e)
        => TryAddCurrentTag();

    /// <summary>
    /// Called when the user presses Return/Enter in the tag entry field.
    /// </summary>
    private void OnTagEntryCompleted(object sender, EventArgs e)
        => TryAddCurrentTag();

    /// <summary>
    /// Reads the tag entry, validates, adds to the list, and rebuilds chips.
    /// </summary>
    private void TryAddCurrentTag()
    {
        var name = TagEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(name)) return;

        // Silently ignore duplicate tag names (case-insensitive)
        if (_selectedTags.Any(t => t.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            TagEntry.Text = string.Empty;
            return;
        }

        _selectedTags.Add(name);
        TagEntry.Text = string.Empty;
        BuildTagChips();
    }

    /// <summary>
    /// Rebuilds the tag chip FlexLayout from _selectedTags list.
    /// Colors cycle through TagColors.Palette by position (index % Palette.Length).
    /// Called after every add or remove so the UI stays in sync with the tag list.
    /// </summary>
    private void BuildTagChips()
    {
        TagChipContainer.Children.Clear();

        for (int i = 0; i < _selectedTags.Count; i++)
        {
            var tagName = _selectedTags[i];

            // Color cycles through TagColors.Palette by position
            var colorHex = TagColors.GetColor(i);

            // The chip: colored pill with tag name + × button
            var chip = new Border
            {
                StrokeThickness = 0,
                BackgroundColor = Color.FromArgb(colorHex),
                Padding = new Thickness(10, 5),
                Margin = new Thickness(0, 0, 6, 6),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
                {
                    CornerRadius = new CornerRadius(12)
                }
            };

            var row = new HorizontalStackLayout { Spacing = 6 };

            row.Children.Add(new Label
            {
                Text = tagName,
                FontFamily = "MontserratMedium",
                FontSize = 12,
                TextColor = Colors.White,
                VerticalOptions = LayoutOptions.Center
            });

            // × button — removes this specific tag
            var removeLabel = new Label
            {
                Text = "×",
                FontFamily = "MontserratBold",
                FontSize = 14,
                TextColor = Colors.White,
                VerticalOptions = LayoutOptions.Center
            };

            var capturedName = tagName; // capture for the closure
            var removeTap = new TapGestureRecognizer();
            removeTap.Tapped += (_, _) =>
            {
                _selectedTags.Remove(capturedName);
                BuildTagChips(); // rebuild so colors re-index correctly
            };
            removeLabel.GestureRecognizers.Add(removeTap);
            row.Children.Add(removeLabel);

            chip.Content = row;
            TagChipContainer.Children.Add(chip);
        }
    }

    private void OnAddCounterTapped(object sender, TappedEventArgs e)
    => TryAddCurrentCounter();

    private void OnCounterEntryCompleted(object sender, EventArgs e)
        => TryAddCurrentCounter();

    private void TryAddCurrentCounter()
    {
        var name = CounterEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name)) return;

        // Silently ignore duplicates (case-insensitive)
        if (_selectedCounterNames.Any(c => c.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            CounterEntry.Text = string.Empty;
            return;
        }

        _selectedCounterNames.Add(name);
        CounterEntry.Text = string.Empty;
        BuildCounterChips();
    }

    private void BuildCounterChips()
    {
        CounterChipContainer.Children.Clear();

        foreach (var counterName in _selectedCounterNames)
        {
            var chip = new Border
            {
                StrokeThickness = 0,
                BackgroundColor = Microsoft.Maui.Controls.Application.Current?.RequestedTheme == AppTheme.Dark
                    ? Color.FromArgb("#4A5259")
                    : Color.FromArgb("#F5EDD3"),
                Padding = new Thickness(10, 6),
                Margin = new Thickness(0, 0, 6, 6),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
                {
                    CornerRadius = new CornerRadius(10)
                }
            };

            var row = new HorizontalStackLayout { Spacing = 6 };

            row.Children.Add(new Label
            {
                Text = counterName,
                FontFamily = "MontserratMedium",
                FontSize = 12,
                TextColor = Microsoft.Maui.Controls.Application.Current?.RequestedTheme == AppTheme.Dark
                    ? Colors.White
                    : Color.FromArgb("#2C3338"),
                VerticalOptions = LayoutOptions.Center
            });

            var removeLabel = new Label
            {
                Text = "×",
                FontFamily = "MontserratBold",
                FontSize = 14,
                TextColor = Color.FromArgb("#E1AD37"),
                VerticalOptions = LayoutOptions.Center
            };

            var captured = counterName;
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) =>
            {
                _selectedCounterNames.Remove(captured);
                BuildCounterChips();
            };
            removeLabel.GestureRecognizers.Add(tap);
            row.Children.Add(removeLabel);

            chip.Content = row;
            CounterChipContainer.Children.Add(chip);
        }
    }

    // ─── File pickers ─────────────────────────────────────────────

    /// <summary>
    /// Opens a choice between camera and photo library.
    /// Copies the chosen photo to the app's local storage folder.
    /// </summary>
    private async void OnPhotoUploadTapped(object sender, TappedEventArgs e)
    {
        try
        {
            var action = await Microsoft.Maui.Controls.Application.Current!.Windows[0].Page!
                .DisplayActionSheet("Add Photo", "Cancel", null, "Take Photo", "Choose from Library");

            if (action == null || action == "Cancel") return;

            FileResult? photo = null;

            if (action == "Take Photo")
            {
                // Check camera permission before attempting — avoids confusing exception
                var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
                if (cameraStatus != PermissionStatus.Granted)
                    cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();

                if (cameraStatus != PermissionStatus.Granted)
                {
                    await ShowAlertAsync(
                        "Camera Permission Needed",
                        "Please allow camera access in your device Settings to take a photo.");
                    return;
                }

                if (MediaPicker.Default.IsCaptureSupported)
                    photo = await MediaPicker.Default.CapturePhotoAsync();
            }
            else
            {
                // Library access — no permission needed on Android 13+ or iOS 14+
                photo = await MediaPicker.Default.PickPhotoAsync();
            }

            if (photo == null) return;

            _selectedImagePath = await CopyFileToLocalStorageAsync(photo.FullPath, "Images");

            PhotoLabel.Text = "Photo Added ✓";
            PhotoFileNameLabel.Text = photo.FileName;
            PhotoFileNameLabel.IsVisible = true;

        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            await ShowAlertAsync("Could Not Add Photo", "Something went wrong. Please try again.");
        }
    }

    /// <summary>
    /// Opens camera/library/PDF picker for a new pattern file.
    /// </summary>
    private async void OnAddPatternFileTapped(object sender, TappedEventArgs e)
    {
        try
        {
            var choice = await ShowActionSheetAsync(
                "Add Pattern", "Cancel", null,
                "Take Photo", "Choose Photo from Library", "Choose PDF");

            if (choice == null || choice == "Cancel") return;

            PendingProjectFile? pending = choice switch
            {
                "Take Photo" => await PickFileAsPhotoAsync(useCamera: true, ProjectFileType.Pattern),
                "Choose Photo from Library" => await PickFileAsPhotoAsync(useCamera: false, ProjectFileType.Pattern),
                "Choose PDF" => await PickFileAsPdfAsync(ProjectFileType.Pattern),
                _ => null
            };

            if (pending == null) return;

            _pendingFiles.Add(pending);
            BuildFileChips(PatternFilesContainer, ProjectFileType.Pattern);
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
        }
    }

    /// <summary>
    /// Opens camera/library picker for a new inspiration photo.
    /// </summary>
    private async void OnAddInspirationPhotoTapped(object sender, TappedEventArgs e)
    {
        try
        {
            var choice = await ShowActionSheetAsync(
                "Add Inspiration Photo", "Cancel", null,
                "Take Photo", "Choose from Library");

            if (choice == null || choice == "Cancel") return;

            var pending = await PickFileAsPhotoAsync(
                useCamera: choice == "Take Photo",
                ProjectFileType.InspirationPhoto);

            if (pending == null) return;

            _pendingFiles.Add(pending);
            BuildFileChips(InspirationPhotosContainer, ProjectFileType.InspirationPhoto);
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
        }
    }

    /// <summary>
    /// Picks a photo (camera or library) and returns a PendingProjectFile.
    /// </summary>
    private static async Task<PendingProjectFile?> PickFileAsPhotoAsync(bool useCamera, ProjectFileType fileType)
    {
        FileResult? result;

        if (useCamera)
        {
            if (!MediaPicker.Default.IsCaptureSupported) return null;

            var status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                await ShowAlertAsync("Camera Permission Needed", "Please allow camera access.");
                return null;
            }
            result = await MediaPicker.Default.CapturePhotoAsync();
        }
        else
        {
            result = await MediaPicker.Default.PickPhotoAsync();
        }

        if (result == null) return null;

        var subfolder = fileType == ProjectFileType.Pattern ? "Patterns" : "Inspiration";
        var destPath = await CopyFileToLocalStorageAsync(result.FullPath, subfolder);
        var fileSize = new FileInfo(result.FullPath).Length;

        var extension = Path.GetExtension(result.FileName).ToUpperInvariant();
        var contentType = extension is ".JPG" or ".JPEG" ? "image/jpeg" : "image/png";

        return new PendingProjectFile(
            ExistingId: null,
            FileName: result.FileName,
            FilePath: destPath,
            FileSizeBytes: fileSize,
            ContentType: contentType,
            FileType: fileType);
    }

    /// <summary>
    /// Opens a PDF file picker and returns a PendingProjectFile.
    /// </summary>
    private static async Task<PendingProjectFile?> PickFileAsPdfAsync(ProjectFileType fileType)
    {
        var options = new PickOptions
        {
            PickerTitle = "Select PDF Pattern",
            FileTypes = PdfFileType
        };

        var result = await FilePicker.Default.PickAsync(options);
        if (result == null) return null;

        var destPath = await CopyFileToLocalStorageAsync(result.FullPath, "Patterns");
        var fileSize = new FileInfo(result.FullPath).Length;

        return new PendingProjectFile(
            ExistingId: null,
            FileName: result.FileName,
            FilePath: destPath,
            FileSizeBytes: fileSize,
            ContentType: "application/pdf",
            FileType: fileType);
    }

    /// <summary>
    /// Rebuilds the 2-column file chip grid for a specific file type.
    /// Called after every add or remove.
    /// </summary>
    private void BuildFileChips(VerticalStackLayout container, ProjectFileType fileType)
    {
        container.Children.Clear();

        var files = _pendingFiles.Where(f => f.FileType == fileType).ToList();
        if (files.Count == 0) return;

        // Pair files into rows of 2 — same pattern as BuildRowNotesGrid
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

            row.Add(CreateFileChip(files[i], fileType, container), column: 0, row: 0);

            if (i + 1 < files.Count)
                row.Add(CreateFileChip(files[i + 1], fileType, container), column: 1, row: 0);
            else
                row.Add(new BoxView { IsVisible = false }, column: 1, row: 0);

            container.Children.Add(row);
        }
    }

    /// <summary>
    /// Creates a file chip showing the filename and a × remove button.
    /// </summary>
    private Border CreateFileChip(
        PendingProjectFile file,
        ProjectFileType fileType,
        VerticalStackLayout parentContainer)
    {
        var chip = new Border
        {
            StrokeThickness = 0,
            BackgroundColor = Color.FromArgb(
                Microsoft.Maui.Controls.Application.Current?.RequestedTheme == AppTheme.Dark
                    ? "#374151" : "#E8E2D0"),
            Padding = new Thickness(10, 8),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
            {
                CornerRadius = new CornerRadius(8)
            }
        };

        var textColor = Microsoft.Maui.Controls.Application.Current?.RequestedTheme == AppTheme.Dark
            ? Colors.White
            : Color.FromArgb("#2C3338");

        var isPdf = file.ContentType == "application/pdf";

        var row = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
        {
            new ColumnDefinition { Width = GridLength.Auto }, // icon
            new ColumnDefinition { Width = GridLength.Star }, // name
            new ColumnDefinition { Width = GridLength.Auto }  // ×
        },
            ColumnSpacing = 6
        };

        // File type icon — always uses light variant (acceptable at 14px in a themed chip)
        var icon = new Image
        {
            WidthRequest = 14,
            HeightRequest = 14,
            VerticalOptions = LayoutOptions.Center,
            Source = isPdf
        ? "file_light.svg"
        : "photo_light.svg"
        };
        row.Add(icon, column: 0, row: 0);

        row.Add(new Label
        {
            Text = file.FileName,
            FontFamily = "MontserratRegular",
            FontSize = 11,
            TextColor = textColor,
            VerticalOptions = LayoutOptions.Center,
            LineBreakMode = LineBreakMode.TailTruncation
        }, column: 1, row: 0);

        var removeLabel = new Label
        {
            Text = "×",
            FontFamily = "MontserratBold",
            FontSize = 16,
            TextColor = textColor,
            Opacity = 0.5,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.End
        };

        var capturedFile = file;
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
        {
            _pendingFiles.Remove(capturedFile);
            BuildFileChips(parentContainer, fileType);
        };
        removeLabel.GestureRecognizers.Add(tap);
        row.Add(removeLabel, column: 2, row: 0);

        chip.Content = row;
        return chip;
    }

    /// <summary>
    /// Copies a file to the app's local data directory under the given subfolder.
    /// Returns the destination path.
    /// </summary>
    private static async Task<string> CopyFileToLocalStorageAsync(string sourcePath, string subfolder)
    {
        // Store under app's local data folder — survives app restarts, not user-accessible
        var destFolder = Path.Combine(FileSystem.AppDataDirectory, subfolder);
        Directory.CreateDirectory(destFolder);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(sourcePath)}";
        var destPath = Path.Combine(destFolder, fileName);

        using var sourceStream = File.OpenRead(sourcePath);
        using var destStream = File.Create(destPath);
        await sourceStream.CopyToAsync(destStream).ConfigureAwait(false);

        return destPath;
    }

    private static async Task ShowAlertAsync(string title, string message)
    {
        await Microsoft.Maui.Controls.Application.Current!.Windows[0].Page!
            .DisplayAlert(title, message, "OK");
    }

    // ─── Save & Cancel ────────────────────────────────────────────

    private async void OnSaveClicked(object sender, TappedEventArgs e)
    {
        var name = NameEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            await ShowAlertAsync("Name Required", "Please enter a project name.");
            return;
        }

        int? totalRows = null;
        if (!string.IsNullOrWhiteSpace(TotalRowsEntry.Text) &&
            int.TryParse(TotalRowsEntry.Text, out var parsed) &&
            parsed > 0)
        {
            totalRows = parsed;
        }

        var result = new ProjectFormResult(
            Name: name,
            ColorHex: _selectedColorHex,
            TotalRows: totalRows,
            Notes: string.IsNullOrWhiteSpace(NotesEditor.Text) ? null : NotesEditor.Text.Trim(),
            NeedleOrHookSize: string.IsNullOrWhiteSpace(NeedleSizeEntry.Text) ? null : NeedleSizeEntry.Text.Trim(),
            Tags: _selectedTags.AsReadOnly(),
            ImagePath: _selectedImagePath,
            ProjectFiles: _pendingFiles.AsReadOnly(),
            InitialCounterNames: _selectedCounterNames.AsReadOnly()  // ← new
        );
        await CloseAsync(result);
    }

    private async void OnCancelClicked(object sender, EventArgs e)
        => await CloseAsync(null);

    private static async Task<string?> ShowActionSheetAsync(
    string title, string cancel, string? destruction, params string[] buttons)
    {
#pragma warning disable CA1826
        var page = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
#pragma warning restore CA1826
        if (page == null) return null;

        return await page.DisplayActionSheet(title, cancel, destruction, buttons)
            .ConfigureAwait(true);
    }
}
