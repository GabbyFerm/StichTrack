// Copyright (c) 2026 Gabriella Frank Ferm / Frank Ferm Design. All rights reserved.
using StitchTrack.Domain.Interfaces;
using StitchTrack.Application.Interfaces;

namespace StitchTrack.MAUI;

public partial class App : Microsoft.Maui.Controls.Application
{
    public App(IAppSettingsRepository settingsRepository, IHapticsService hapticsService)
    {
        ArgumentNullException.ThrowIfNull(settingsRepository);
        ArgumentNullException.ThrowIfNull(hapticsService);

        InitializeComponent();

        _ = ApplyStoredSettingsAsync(settingsRepository, hapticsService);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }

    /// <summary>
    /// Reads AppSettings from the database and applies theme + haptics immediately.
    /// Fire-and-forget — the app starts on Auto theme until settings load (usually instant).
    /// </summary>
    private static async Task ApplyStoredSettingsAsync(
        IAppSettingsRepository settingsRepository,
        IHapticsService hapticsService)
    {
        try
        {
            var settings = await settingsRepository.GetAppSettingsAsync().ConfigureAwait(false);

            // Apply theme on the main thread since it touches the UI
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Microsoft.Maui.Controls.Application.Current!.UserAppTheme = settings.Theme switch
                {
                    "Light" => AppTheme.Light,
                    "Dark" => AppTheme.Dark,
                    _ => AppTheme.Unspecified
                };
            }).ConfigureAwait(false);

            // Sync haptics service with persisted value
            hapticsService.IsEnabled = settings.HapticFeedbackEnabled;

        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            // Safe fallback — Auto theme, haptics on
        }
    }
}
