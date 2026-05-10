using StitchTrack.Application.ViewModels;

namespace StitchTrack.MAUI.Views;

/// <summary>
/// Export page — allows users to export project data as JSON or CSV.
/// All logic lives in ExportViewModel — this code-behind only wires up the BindingContext.
/// </summary>
public partial class ExportPage : ContentPage
{
    public ExportPage(ExportViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }
}
