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
    /// Shows a simple alert with a dismiss button.
    /// </summary>
    Task ShowAlertAsync(string title, string message, string cancel = "OK");

    /// <summary>
    /// Shows a brief success/info message.
    /// </summary>
    Task ShowToastAsync(string message);

    /// <summary>
    /// Shows a native action sheet (bottom sheet with labelled options).
    /// Returns the label of the button the user tapped, or null if cancelled.
    /// 
    /// - <paramref name="destruction"/> is shown in red (use for destructive actions like Delete).
    ///   Pass null if there is no destructive option.
    /// - <paramref name="buttons"/> are the non-destructive options shown above the cancel button.
    /// </summary>
    Task<string?> ShowActionSheetAsync(
        string title,
        string cancel,
        string? destruction,
        params string[] buttons);
}
