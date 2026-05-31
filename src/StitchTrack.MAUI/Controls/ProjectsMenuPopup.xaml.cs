// Copyright (c) 2026 Gabriella Frank Ferm / Frank Ferm Design. All rights reserved.
using CommunityToolkit.Maui.Views;
using StitchTrack.Domain.Entities;

namespace StitchTrack.MAUI.Controls;

/// <summary>
/// Custom bottom sheet menu for project actions.
/// Replaces the native action sheet with a branded UI.
///
/// Returns a string: "Edit", "Archive", "Unarchive", "Delete", or null for cancel.
/// </summary>
public partial class ProjectMenuPopup : Popup
{
    private readonly bool _isArchived;

    public ProjectMenuPopup(Project project)
    {
        ArgumentNullException.ThrowIfNull(project);

        InitializeComponent();

        _isArchived = project.IsArchived;

        // Set the project name in the header
        ProjectNameLabel.Text = project.Name;

        // Toggle label based on whether project is already archived
        ArchiveLabel.Text = _isArchived ? "Unarchive" : "Archive";
    }

    private async void OnEditTapped(object sender, TappedEventArgs e)
        => await CloseAsync("Edit");

    private async void OnArchiveTapped(object sender, TappedEventArgs e)
        => await CloseAsync(_isArchived ? "Unarchive" : "Archive");

    private async void OnDeleteTapped(object sender, TappedEventArgs e)
        => await CloseAsync("Delete");

    private async void OnCancelTapped(object sender, TappedEventArgs e)
        => await CloseAsync(null);
}
