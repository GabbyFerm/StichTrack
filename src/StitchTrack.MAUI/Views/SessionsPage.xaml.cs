using StitchTrack.Application.ViewModels;

namespace StitchTrack.MAUI.Views;

public partial class SessionsPage : ContentPage
{
    private readonly SessionsViewModel _viewModel;

    public SessionsPage(SessionsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = _viewModel;
    }

    /// <summary>
    /// Reload sessions every time the tab is shown —
    /// covers the case where a new session was completed on another tab.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadSessionsAsync();
    }
}
