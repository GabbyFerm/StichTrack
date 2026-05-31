using CommunityToolkit.Maui.Views;
using StitchTrack.Application.Models;
using StitchTrack.Application.ViewModels;
using StitchTrack.Domain.Entities;
using StitchTrack.MAUI.Controls;

namespace StitchTrack.MAUI.Views;

/// <summary>
/// Code-behind for ProjectsPage.
/// Owns popup lifecycle — the ViewModel stays free of any MAUI UI dependencies.
/// </summary>
public partial class ProjectsPage : ContentPage
{
    private readonly ProjectsViewModel _viewModel;

    public ProjectsPage(ProjectsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = _viewModel;

        // Wire popup callbacks — ViewModel requests UI, Page delivers it
        _viewModel.ShowProjectFormAsync = ShowProjectFormPopupAsync;
        _viewModel.ShowProjectMenuPopupAsync = ShowProjectMenuPopupAsync;

    }

    /// <summary>
    /// Reload projects every time the page appears —
    /// covers the case where a project was edited on SingleProjectPage.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadProjectsAsync();
    }

    /// <summary>
    /// Opens the ProjectFormPopup.
    /// Called by the ViewModel via the ShowProjectFormAsync callback.
    ///
    /// - project == null  →  create mode (empty form)
    /// - project != null  →  edit mode (pre-filled form)
    ///
    /// Returns the form result, or null if the user cancelled.
    /// </summary>
    private async Task<ProjectFormResult?> ShowProjectFormPopupAsync(Project? project)
    {
        ProjectFormResult? formResult = null;

        // ShowPopupAsync requires the main thread
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var popup = new ProjectFormPopup(project);
            var result = await this.ShowPopupAsync(popup);
            formResult = result as ProjectFormResult;
        });

        return formResult;
    }

    /// <summary>
    /// Opens the ProjectMenuPopup and returns the action the user tapped,
    /// or null if they cancelled.
    /// </summary>
    private async Task<string?> ShowProjectMenuPopupAsync(Project project)
    {
        string? action = null;

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var popup = new ProjectMenuPopup(project);
            var result = await this.ShowPopupAsync(popup);
            action = result as string;
        });

        // Delay outside the UI thread block — no reason to block the UI for this
        await Task.Delay(300);

        return action;
    }
}
