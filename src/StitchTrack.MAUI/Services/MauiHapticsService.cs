// Copyright (c) 2026 Gabriella Frank Ferm / Frank Ferm Design. All rights reserved.
using StitchTrack.Application.Interfaces;

namespace StitchTrack.MAUI.Services;

/// <summary>
/// MAUI implementation of IHapticsService.
/// Checks IsEnabled before firing so all callers stay clean.
/// </summary>
public class MauiHapticsService : IHapticsService
{
    // Defaults to true — overwritten by SettingsViewModel on startup
    public bool IsEnabled { get; set; } = true;

    public void Click()
    {
        if (!IsEnabled) return;

        // Prefer haptic click, fall back to short vibration
        try
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            return;
        }
        catch (FeatureNotSupportedException) { }
        catch (InvalidOperationException) { }
        catch (OperationCanceledException) { }

        try
        {
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(10));
        }
        catch (FeatureNotSupportedException) { }
        catch (PermissionException) { }
        catch (InvalidOperationException) { }
    }
}
