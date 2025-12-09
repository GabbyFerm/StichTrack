using Microsoft.EntityFrameworkCore;
using StitchTrack.Domain.Entities;
using StitchTrack.Domain.Interfaces;
using StitchTrack.Infrastructure.Data;

namespace StitchTrack.Infrastructure.Repositories;

/// <summary>
/// Repository for AppSettings (single-row table).
/// </summary>
public class AppSettingsRepository : IAppSettingsRepository
{
    private readonly AppDbContext _context;

    public AppSettingsRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets the single AppSettings instance.
    /// Creates default if not exists (first-run scenario).
    /// </summary>
    public async Task<AppSettings> GetAppSettingsAsync()
    {
        // Try to get existing settings
        var settings = await _context.AppSettings
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        // If no settings exist, create default
        if (settings == null)
        {
            settings = AppSettings.CreateDefault();
            _context.AppSettings.Add(settings);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            System.Diagnostics.Debug.WriteLine("✨ Created default AppSettings");
        }

        return settings;
    }

    /// <summary>
    /// Saves AppSettings changes to database.
    /// </summary>
    public async Task SaveAppSettingsAsync(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        // Update existing entry
        _context.AppSettings.Update(settings);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        System.Diagnostics.Debug.WriteLine("💾 Saved AppSettings");
    }
}
