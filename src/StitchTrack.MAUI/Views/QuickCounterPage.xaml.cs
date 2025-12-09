using StitchTrack.Application.ViewModels;

namespace StitchTrack.MAUI.Views;

public partial class QuickCounterPage : ContentPage
{
    public QuickCounterPage()
    {
        InitializeComponent();

        // Wire up haptic feedback to ViewModel
        if (BindingContext is QuickCounterViewModel viewModel)
        {
            viewModel.TriggerHapticFeedback = () =>
            {
                // Light haptic feedback for button press
                // MAUI's HapticFeedback works on iOS and Android
                HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            };
        }
    }
}
