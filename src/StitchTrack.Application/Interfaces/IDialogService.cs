namespace StitchTrack.Application.Interfaces;

/// <summary>
/// Service interface for showing user dialogs and prompts.
/// Abstracts MAUI-specific UI operations from the Application layer.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows a prompt dialog asking the user to enter text.
    /// </summary>
    /// <param name="title">Dialog title</param>
    /// <param name="message">Dialog message</param>
    /// <param name="accept">Accept button text (e.g., "Save")</param>
    /// <param name="cancel">Cancel button text (e.g., "Cancel")</param>
    /// <param name="placeholder">Placeholder text for input field</param>
    /// <param name="maxLength">Maximum length of input</param>
    /// <returns>The entered text, or null if cancelled</returns>
    Task<string?> ShowPromptAsync(
        string title,
        string message,
        string accept = "OK",
        string cancel = "Cancel",
        string? placeholder = null,
        int maxLength = -1);

    /// <summary>
    /// Shows an alert dialog with a message.
    /// </summary>
    /// <param name="title">Alert title</param>
    /// <param name="message">Alert message</param>
    /// <param name="cancel">Button text (e.g., "OK")</param>
    Task ShowAlertAsync(string title, string message, string cancel = "OK");

    /// <summary>
    /// Shows a brief toast-style message (simulated in Phase 1).
    /// </summary>
    /// <param name="message">Message to display</param>
    Task ShowToastAsync(string message);
}
