using System;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Views;

namespace StitchTrack.MAUI.Controls;

public partial class OnboardingPopup : Popup
{
    public OnboardingPopup()
    {
        InitializeComponent();

        BtnGetStarted.Clicked += async (_, _) => await ExecuteVmCommandAndCloseAsync("GetStartedCommand");
        BtnEnableSync.Clicked += async (_, _) => await ExecuteVmCommandAndCloseAsync("EnableSyncCommand");
        BtnMaybeLater.Clicked += async (_, _) => await ExecuteVmCommandAndCloseAsync("MaybeLaterCommand");
    }

    // Looks up the named ICommand on the binding context and executes it (if present),
    // then closes the popup.
    private async Task ExecuteVmCommandAndCloseAsync(string commandPropertyName)
    {
        try
        {
            var vm = BindingContext;
            if (vm != null)
            {
                var prop = vm.GetType().GetProperty(commandPropertyName);
                if (prop?.GetValue(vm) is System.Windows.Input.ICommand cmd && cmd.CanExecute(null))
                {
                    // Execute on the UI thread in a safe manner using fully-qualified MainThread
                    await global::Microsoft.Maui.ApplicationModel.MainThread.InvokeOnMainThreadAsync(() => cmd.Execute(null));
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnboardingPopup: ExecuteVmCommand error: {ex}");
        }

        try { Close(null); } catch { }
    }
}
