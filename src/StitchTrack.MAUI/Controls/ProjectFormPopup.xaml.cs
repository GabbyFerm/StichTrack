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
                WidthRequest = 36,
                HeightRequest = 36,
                StrokeThickness = _selectedColorHex == hex ? 2 : 0,
                Stroke = Color.FromArgb("#F59E0B"),
                BackgroundColor = Colors.Transparent,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.Ellipse()
            };

            var dot = new Border
            {
                WidthRequest = 28,
                HeightRequest = 28,
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
            var options = new PickOptions
            {
                PickerTitle = "Select PDF Pattern",
                FileTypes = PdfFileType
            };

            var result = await FilePicker.Default.PickAsync(options);
            if (result == null) return;

            // Copy to app's local storage
            _selectedPatternFilePath = await CopyFileToLocalStorageAsync(result.FullPath, "Patterns");

            PatternLabel.Text = "📄 Pattern Added ✓";
            PatternFileNameLabel.Text = result.FileName;
            PatternFileNameLabel.IsVisible = true;

            System.Diagnostics.Debug.WriteLine($"📄 Pattern saved: {_selectedPatternFilePath}");
        }
        catch (PermissionException)
        {
            await ShowAlertAsync("Permission Needed", "Please allow file access in Settings.");
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            System.Diagnostics.Debug.WriteLine($"❌ Pattern upload error: {ex.Message}");
        }
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
            ImagePath: _selectedImagePath,
            PatternFilePath: _selectedPatternFilePath,
            PatternFileName: PatternFileNameLabel.Text
        );

        await CloseAsync(result);
    }

    private async void OnCancelClicked(object sender, EventArgs e)
        => await CloseAsync(null);
}
