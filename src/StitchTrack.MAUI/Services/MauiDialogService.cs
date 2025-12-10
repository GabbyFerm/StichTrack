using StitchTrack.Application.Interfaces;

namespace StitchTrack.MAUI.Services;

/// <summary>
/// MAUI implementation of IDialogService.
/// Uses MAUI's built-in dialog APIs to show prompts and alerts.
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
        if (Microsoft.Maui.Controls.Application.Current?.MainPage == null)
        {
            System.Diagnostics.Debug.WriteLine("⚠️ Cannot show dialog: MainPage is null");
            return null;
        }

        return await Microsoft.Maui.Controls.Application.Current.MainPage.DisplayPromptAsync(
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

        if (Microsoft.Maui.Controls.Application.Current?.MainPage == null)
        {
            System.Diagnostics.Debug.WriteLine("⚠️ Cannot show alert: MainPage is null");
            return;
        }

        await Microsoft.Maui.Controls.Application.Current.MainPage.DisplayAlert(
            title,
            message,
            cancel
        ).ConfigureAwait(false);
    }

    public async Task ShowToastAsync(string message)
    {
        ArgumentNullException.ThrowIfNull(message);

        // TODO: In Phase 2, implement proper toast notifications
        // For now, use a brief alert
        if (Microsoft.Maui.Controls.Application.Current?.MainPage == null)
        {
            System.Diagnostics.Debug.WriteLine("⚠️ Cannot show toast: MainPage is null");
            return;
        }

        await Microsoft.Maui.Controls.Application.Current.MainPage.DisplayAlert(
            "Success",
            message,
            "OK"
        ).ConfigureAwait(false);
    }
}
