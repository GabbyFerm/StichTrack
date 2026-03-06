using CommunityToolkit.Maui.Views;
using StitchTrack.Application.Models;
using StitchTrack.Domain.Entities;

namespace StitchTrack.MAUI.Controls;

/// <summary>
/// Reusable popup for creating a new project or editing an existing one.
/// - Create mode: pass null for project
/// - Edit mode:   pass the existing Project to pre-fill the form
///
/// Returns a ProjectFormResult when saved, or null when cancelled.
/// The caller is responsible for persisting the result.
/// </summary>
public partial class ProjectFormPopup : Popup
{
    // Tracks which color dot the user has selected
    private string _selectedColorHex;

    // True when editing an existing project, false when creating a new one
    private readonly bool _isEditMode;

    public ProjectFormPopup(Project? existingProject = null)
    {
        InitializeComponent();

        _isEditMode = existingProject != null;

        // Set mode-specific labels
        FormTitleLabel.Text = _isEditMode ? "Edit Project" : "New Project";
        SaveButtonLabel.Text = _isEditMode ? "SAVE" : "CREATE";

        // Seed color selection — use existing color or a random one for new projects
        _selectedColorHex = existingProject?.ColorHex ?? ProjectColors.GetRandomColor();

        // Build the color dot picker from the palette
        BuildColorPicker();

        // Pre-fill fields when editing
        if (_isEditMode && existingProject != null)
        {
            NameEntry.Text = existingProject.Name;
            TotalRowsEntry.Text = existingProject.TotalRows?.ToString(System.Globalization.CultureInfo.InvariantCulture);
            NotesEditor.Text = existingProject.Notes;
        }
    }

    /// <summary>
    /// Dynamically builds the color dot row from ProjectColors.Palette.
    /// Adds a visual "selected" ring around the active color.
    /// </summary>
    private void BuildColorPicker()
    {
        ColorPickerContainer.Children.Clear();

        foreach (var hex in ProjectColors.Palette)
        {
            // Outer border acts as the selection ring
            var ring = new Border
            {
                WidthRequest = 36,
                HeightRequest = 36,
                StrokeThickness = _selectedColorHex == hex ? 2 : 0,
                Stroke = Color.FromArgb("#F59E0B"), // BrandGold ring
                BackgroundColor = Colors.Transparent,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.Ellipse()
            };

            // Inner colored dot
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

            // Capture hex in local var so the lambda closes over the right value
            var capturedHex = hex;
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (_, _) => OnColorSelected(capturedHex);
            ring.GestureRecognizers.Add(tapGesture);

            ColorPickerContainer.Children.Add(ring);
        }
    }

    /// <summary>
    /// Updates the selected color and redraws the picker to reflect the new selection ring.
    /// </summary>
    private void OnColorSelected(string hex)
    {
        _selectedColorHex = hex;
        BuildColorPicker(); // Rebuild so only the selected dot has the ring
    }

    /// <summary>
    /// Save tapped — validates the form, then closes with the result.
    /// The ViewModel handles persistence after receiving the result.
    /// </summary>
    private async void OnSaveClicked(object sender, TappedEventArgs e)
    {
        var name = NameEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            // Keep it simple — inline feedback without closing the popup
            await Microsoft.Maui.Controls.Application.Current!.MainPage!.DisplayAlert(
                "Name Required",
                "Please enter a project name.",
                "OK"
            );
            return;
        }

        // Parse total rows — null if empty or invalid
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
            Notes: string.IsNullOrWhiteSpace(NotesEditor.Text) ? null : NotesEditor.Text.Trim()
        );

        // Pass result back to the caller (Page code-behind)
        await CloseAsync(result);
    }

    /// <summary>
    /// Cancel or X tapped — closes without returning a result.
    /// Caller receives null and does nothing.
    /// </summary>
    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await CloseAsync(null);
    }

    /// <summary>
    /// Photo upload tapped — placeholder until Phase 2.
    /// </summary>
    private async void OnPhotoUploadTapped(object sender, TappedEventArgs e)
    {
        await Microsoft.Maui.Controls.Application.Current!.MainPage!.DisplayAlert(
            "Coming Soon",
            "Photo upload will be available in Phase 2.",
            "OK"
        );
    }
}
