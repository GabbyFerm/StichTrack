using CommunityToolkit.Maui.Views;
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
        ArgumentNullException.ThrowIfNull(viewModel);

        InitializeComponent();

        BindingContext = viewModel;

        viewModel.TriggerHapticFeedback = () => global::Microsoft.Maui.Devices.HapticFeedback.Default.Perform(global::Microsoft.Maui.Devices.HapticFeedbackType.Click);

        viewModel.OnProjectSaved = async () => await ShowSaveHintAsync();

        System.Diagnostics.Debug.WriteLine("✅ QuickCounterPage initialized with ViewModel");
    }

    private async Task ShowSaveHintAsync()
    {
        SaveHintFrame.IsVisible = true;
        await SaveHintFrame.FadeTo(1, 300);
        await Task.Delay(4000);
        await SaveHintFrame.FadeTo(0, 300);
        SaveHintFrame.IsVisible = false;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = ShowOnboardingIfNeededAsync();
    }

    private async Task ShowOnboardingIfNeededAsync()
    {
        try
        {
            await Task.Delay(50).ConfigureAwait(false);

            if (BindingContext is QuickCounterViewModel vm && vm.ShowOnboarding)
            {
                await global::Microsoft.Maui.ApplicationModel.MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        var popup = new StitchTrack.MAUI.Controls.OnboardingPopup
                        {
                            BindingContext = vm
                        };
                        // Use fully-qualified Application to avoid namespace collisions
                        await global::Microsoft.Maui.Controls.Application.Current.MainPage.ShowPopupAsync(popup);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"ShowOnboardingIfNeededAsync.ShowPopup error: {ex}");
                    }
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ShowOnboardingIfNeededAsync error: {ex}");
        }
    }
}
