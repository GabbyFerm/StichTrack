// Copyright (c) 2026 Gabriella Frank Ferm / Frank Ferm Design. All rights reserved.
using StitchTrack.Domain.Entities;

namespace StitchTrack.Domain.Interfaces;

/// <summary>
/// Repository interface for AppSettings (single-row table).
/// </summary>
public interface IAppSettingsRepository
{
    /// <summary>
    /// Gets the single AppSettings instance.
    /// Creates default if not exists.
    /// </summary>
    Task<AppSettings> GetAppSettingsAsync();

    /// <summary>
    /// Saves AppSettings changes to database.
    /// </summary>
    Task SaveAppSettingsAsync(AppSettings settings);
}
