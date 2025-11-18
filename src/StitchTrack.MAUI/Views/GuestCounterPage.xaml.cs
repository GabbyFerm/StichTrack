using StitchTrack.Application.ViewModels;

namespace StitchTrack.MAUI.Views;

public partial class GuestCounterPage : Microsoft.Maui.Controls.ContentPage
{
    public GuestCounterPage()
    {
        InitializeComponent();

        // Wire up haptic feedback
        if (BindingContext is GuestCounterViewModel viewModel)
        {
            viewModel.TriggerHapticFeedback = () =>
            {
                // Light haptic feedback for button press
                HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            };
        }
    }
}
