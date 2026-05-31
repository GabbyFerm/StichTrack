// Copyright (c) 2026 Gabriella Frank Ferm / Frank Ferm Design. All rights reserved.
namespace StitchTrack.Application.Interfaces;

/// <summary>
/// Service for triggering haptic feedback.
/// IsEnabled is set by SettingsViewModel on load and toggle —
/// all callers just call Click() and the service decides whether to fire.
/// </summary>
public interface IHapticsService
{
    /// <summary>
    /// Controls whether haptic feedback fires.
    /// Loaded from AppSettings at startup and updated when the user toggles it.
    /// </summary>
    bool IsEnabled { get; set; }

    /// <summary>
    /// Triggers a haptic click if IsEnabled is true.
    /// </summary>
    void Click();
}
