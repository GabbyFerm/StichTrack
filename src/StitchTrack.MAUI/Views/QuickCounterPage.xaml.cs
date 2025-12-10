using StitchTrack.Application.ViewModels;

namespace StitchTrack.MAUI.Views;

/// <summary>
/// Code-behind for QuickCounterPage.
/// Uses constructor injection to get ViewModel from DI container.
/// </summary>
public partial class QuickCounterPage : ContentPage
{
    public QuickCounterPage(QuickCounterViewModel viewModel)
    {
        // Null validation required by CA1062
        ArgumentNullException.ThrowIfNull(viewModel);

        InitializeComponent();

        // Set ViewModel from DI container
        BindingContext = viewModel;

        // Wire up haptic feedback
        viewModel.TriggerHapticFeedback = () =>
        {
            // Light haptic feedback for button press
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        };

        // Wire up save hint display (Issue #14)
        viewModel.OnProjectSaved = async () =>
        {
            await ShowSaveHintAsync();
        };
    }

    /// <summary>
    /// Shows visual hint after saving project (Issue #14)
    /// </summary>
    private async Task ShowSaveHintAsync()
    {
        // Show hint
        SaveHintFrame.IsVisible = true;

        // Animate in
        await SaveHintFrame.FadeTo(1, 300);

        // Wait 4 seconds
        await Task.Delay(4000);

        // Animate out
        await SaveHintFrame.FadeTo(0, 300);
        SaveHintFrame.IsVisible = false;
    }
}
