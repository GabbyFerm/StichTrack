using StitchTrack.Application.Interfaces;

namespace StitchTrack.MAUI.Services;

/// <summary>
/// MAUI implementation of IDialogService.
/// Uses MAUI's built-in dialog APIs to show prompts, alerts, and action sheets.
/// </summary>
public class MauiDialogService : IDialogService
{
    /// <summary>
    /// Safe single-window page lookup.
    /// Returns null during early startup or if no window is available.
    /// </summary>
    private static Page? GetCurrentPage()
    {
        var app = Microsoft.Maui.Controls.Application.Current;
        if (app == null) return null;

        // CA1826 suppressed: FirstOrDefault() is intentional here —
        // Windows[0] throws if the collection is empty during early startup
#pragma warning disable CA1826
        return app.Windows.FirstOrDefault()?.Page;
#pragma warning restore CA1826
    }

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

        var page = GetCurrentPage();
        if (page == null)
        {
            return null;
        }

        return await page.DisplayPromptAsync(
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

        var page = GetCurrentPage();
        if (page == null)
        {
            return;
        }

        await page.DisplayAlert(title, message, cancel).ConfigureAwait(false);
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

        var page = GetCurrentPage();
        if (page == null)
        {
            return false;
        }

        return await MainThread.InvokeOnMainThreadAsync(async () =>
            await page.DisplayAlert(title, message, accept, cancel)
        );
    }

    public async Task ShowToastAsync(string message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var page = GetCurrentPage();
        if (page == null)
        {
            return;
        }

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

        var page = GetCurrentPage();
        if (page == null)
        {
            return null;
        }

        string? result = null;

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            result = await page.DisplayActionSheet(title, cancel, destruction, buttons);
        });

        return result == cancel ? null : result;
    }
}
