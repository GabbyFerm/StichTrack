namespace StitchTrack.Application.Interfaces;

/// <summary>
/// Abstraction for showing dialogs and user feedback.
/// Keeps ViewModels decoupled from MAUI-specific UI APIs.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows a text input prompt and returns what the user typed, or null if cancelled.
    /// </summary>
    Task<string?> ShowPromptAsync(
        string title,
        string message,
        string accept = "OK",
        string cancel = "Cancel",
        string? placeholder = null,
        int maxLength = -1);

    /// <summary>
    /// Shows a simple alert with a dismiss button.
    /// </summary>
    Task ShowAlertAsync(string title, string message, string cancel = "OK");

    /// <summary>
    /// Shows a yes/no confirmation dialog.
    /// Returns true if the user tapped accept, false if cancelled.
    /// Use for reversible actions (e.g. Archive).
    /// Use ShowPromptAsync with typed confirmation for permanent actions (e.g. Delete).
    /// </summary>
    Task<bool> ShowConfirmAsync(
        string title,
        string message,
        string accept = "Yes",
        string cancel = "Cancel");

    /// <summary>
    /// Shows a brief success/info message.
    /// </summary>
    Task ShowToastAsync(string message);

    /// <summary>
    /// Shows a native action sheet (bottom sheet with labelled options).
    /// Returns the label of the button the user tapped, or null if cancelled.
    /// destruction is shown in red — pass null if there is no destructive option.
    /// </summary>
    Task<string?> ShowActionSheetAsync(
        string title,
        string cancel,
        string? destruction,
        params string[] buttons);
}
