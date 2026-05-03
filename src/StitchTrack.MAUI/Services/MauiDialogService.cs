using StitchTrack.Application.Interfaces;

namespace StitchTrack.MAUI.Services;

/// <summary>
/// MAUI implementation of IDialogService.
/// Uses MAUI's built-in dialog APIs to show prompts, alerts, and action sheets.
/// </summary>
public class MauiDialogService : IDialogService
{
    public async Task<string?> ShowPromptAsync(
        string title,
        string message,
        string accept = "OK",
        string cancel = "Cancel",
        string? placeholder = null,
        int maxLength = -1)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(message);

        // Ensure we're on the main thread
        if (Microsoft.Maui.Controls.Application.Current?.Windows[0].Page == null)
        {
            System.Diagnostics.Debug.WriteLine("⚠️ Cannot show prompt: MainPage is null");
            return null;
        }

        return await Microsoft.Maui.Controls.Application.Current.Windows[0].Page!.DisplayPromptAsync(
            title: title,
            message: message,
            accept: accept,
            cancel: cancel,
            placeholder: placeholder,
            maxLength: maxLength,
            keyboard: Keyboard.Text
        ).ConfigureAwait(false);
    }

    public async Task ShowAlertAsync(string title, string message, string cancel = "OK")
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(message);

        if (Microsoft.Maui.Controls.Application.Current?.Windows[0].Page == null)
        {
            System.Diagnostics.Debug.WriteLine("⚠️ Cannot show alert: MainPage is null");
            return;
        }

        await Microsoft.Maui.Controls.Application.Current.Windows[0].Page!.DisplayAlert(
            title,
            message,
            cancel
        ).ConfigureAwait(false);
    }

    /// <summary>
    /// Shows a two-button confirmation dialog.
    /// Returns true if the user tapped accept, false otherwise.
    /// </summary>
    public async Task<bool> ShowConfirmAsync(
        string title,
        string message,
        string accept = "Yes",
        string cancel = "Cancel")
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(message);

        if (Microsoft.Maui.Controls.Application.Current?.Windows[0].Page == null)
        {
            System.Diagnostics.Debug.WriteLine("⚠️ Cannot show confirm: MainPage is null");
            return false;
        }

        return await MainThread.InvokeOnMainThreadAsync(async () =>
            await Microsoft.Maui.Controls.Application.Current.Windows[0].Page!.DisplayAlert(
                title, message, accept, cancel
            )
        );
    }

    public async Task ShowToastAsync(string message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (Microsoft.Maui.Controls.Application.Current?.Windows[0].Page == null)
        {
            System.Diagnostics.Debug.WriteLine("⚠️ Cannot show toast: MainPage is null");
            return;
        }

        // CommunityToolkit Toast — auto-dismisses, no button needed
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var toast = CommunityToolkit.Maui.Alerts.Toast.Make(
                message,
                CommunityToolkit.Maui.Core.ToastDuration.Short
            );
            await toast.Show();
        });
    }

    /// <summary>
    /// Shows a native bottom action sheet.
    /// Returns the label tapped by the user, or null if they tapped cancel.
    /// </summary>
    public async Task<string?> ShowActionSheetAsync(
        string title,
        string cancel,
        string? destruction,
        params string[] buttons)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(cancel);

        if (Microsoft.Maui.Controls.Application.Current?.Windows[0].Page == null)
        {
            System.Diagnostics.Debug.WriteLine("⚠️ Cannot show action sheet: MainPage is null");
            return null;
        }

        string? result = null;

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            result = await Microsoft.Maui.Controls.Application.Current.Windows[0].Page!.DisplayActionSheet(
                title, cancel, destruction, buttons
            );
        });

        return result == cancel ? null : result;
    }
}
