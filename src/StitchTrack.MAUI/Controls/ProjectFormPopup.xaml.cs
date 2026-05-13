using CommunityToolkit.Maui.Views;
using StitchTrack.Application.Models;
using StitchTrack.Domain.Entities;

namespace StitchTrack.MAUI.Controls;

/// <summary>
/// Reusable popup for creating or editing a project.
/// Handles photo upload (camera/library) and PDF pattern upload (file picker).
/// Returns a ProjectFormResult on save, or null on cancel.
/// </summary>
public partial class ProjectFormPopup : Popup
{
    private string _selectedColorHex;
    private string? _selectedImagePath;
    private string? _selectedPatternFilePath;
    private readonly bool _isEditMode;

    // Tracks tag names in the order the user added them
    private readonly List<string> _selectedTags = new();


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

            // Show existing image name if set
            if (!string.IsNullOrWhiteSpace(existingProject.ImagePath))
            {
                PhotoFileNameLabel.Text = Path.GetFileName(existingProject.ImagePath);
                PhotoFileNameLabel.IsVisible = true;
                PhotoLabel.Text = "📷 Change Photo";
            }

            // Show existing pattern name if any
            var existingPattern = existingProject.PatternFiles.FirstOrDefault();
            if (existingPattern != null && !string.IsNullOrWhiteSpace(existingPattern.FilePath))
            {
                _selectedPatternFilePath = existingPattern.FilePath;
                PatternFileNameLabel.Text = existingPattern.FileName;
                PatternFileNameLabel.IsVisible = true;
                PatternLabel.Text = "📄 Change Pattern";
            }

            // Pre-fill needle/hook size
            NeedleSizeEntry.Text = existingProject.NeedleOrHookSize;

            // Pre-fill tags — project must be loaded with .Include(p => p.Tags)
            foreach (var tag in existingProject.Tags.OrderBy(t => t.ColorIndex))
                _selectedTags.Add(tag.Name);

            BuildTagChips();
        }
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
    /// Rebuilds the chip FlexLayout from _selectedTags.
    /// Called after every add or remove so the UI stays in sync.
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

            PhotoLabel.Text = "📷 Photo Added ✓";
            PhotoFileNameLabel.Text = photo.FileName;
            PhotoFileNameLabel.IsVisible = true;

            System.Diagnostics.Debug.WriteLine($"📷 Photo saved: {_selectedImagePath}");
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            System.Diagnostics.Debug.WriteLine($"❌ Photo upload error: {ex.Message}");
            await ShowAlertAsync("Could Not Add Photo", "Something went wrong. Please try again.");
        }
    }

    /// <summary>
    /// Opens the file picker filtered to PDF files.
    /// Copies the chosen PDF to the app's local storage folder.
    /// </summary>
    private async void OnPatternUploadTapped(object sender, TappedEventArgs e)
    {
        try
        {
            // Let user choose how to add the pattern
            var choice = await ShowActionSheetAsync(
                "Add Pattern",
                "Cancel",
                null,
                "Take Photo",
                "Choose Photo from Library",
                "Choose PDF");

            if (choice == null || choice == "Cancel") return;

            switch (choice)
            {
                case "Take Photo":
                    await PickPatternPhotoAsync(useCamera: true);
                    break;
                case "Choose Photo from Library":
                    await PickPatternPhotoAsync(useCamera: false);
                    break;
                case "Choose PDF":
                    await PickPatternPdfAsync();
                    break;
            }
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            System.Diagnostics.Debug.WriteLine($"❌ Pattern upload error: {ex.Message}");
        }
    }

    private async Task PickPatternPhotoAsync(bool useCamera)
    {
        FileResult? result;
        if (useCamera)
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await ShowAlertAsync("Camera Not Available", "This device does not support taking photos.");
                return;
            }
            var cameraStatus = await Microsoft.Maui.ApplicationModel.Permissions.RequestAsync<Microsoft.Maui.ApplicationModel.Permissions.Camera>();
            if (cameraStatus != Microsoft.Maui.ApplicationModel.PermissionStatus.Granted)
            {
                await ShowAlertAsync("Camera Permission Needed", "Please allow camera access to take a pattern photo.");
                return;
            }
            result = await MediaPicker.Default.CapturePhotoAsync();
        }
        else
        {
            result = await MediaPicker.Default.PickPhotoAsync();
        }

        if (result == null) return;

        _selectedPatternFilePath = await CopyFileToLocalStorageAsync(result.FullPath, "Patterns");
        PatternLabel.Text = "🖼️ Pattern Added ✓";
        PatternFileNameLabel.Text = result.FileName;
        PatternFileNameLabel.IsVisible = true;

        System.Diagnostics.Debug.WriteLine($"🖼️ Pattern photo saved: {_selectedPatternFilePath}");
    }

    private async Task PickPatternPdfAsync()
    {
        var options = new PickOptions
        {
            PickerTitle = "Select PDF Pattern",
            FileTypes = PdfFileType
        };

        var result = await FilePicker.Default.PickAsync(options);
        if (result == null) return;

        _selectedPatternFilePath = await CopyFileToLocalStorageAsync(result.FullPath, "Patterns");
        PatternLabel.Text = "📄 Pattern Added ✓";
        PatternFileNameLabel.Text = result.FileName;
        PatternFileNameLabel.IsVisible = true;

        System.Diagnostics.Debug.WriteLine($"📄 Pattern PDF saved: {_selectedPatternFilePath}");
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
            Tags: _selectedTags.AsReadOnly(),  // snapshot of the current tag list
            ImagePath: _selectedImagePath,
            PatternFilePath: _selectedPatternFilePath,
            PatternFileName: PatternFileNameLabel.Text
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
