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
    }
}
