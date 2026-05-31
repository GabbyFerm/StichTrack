// Copyright (c) 2026 Gabriella Frank Ferm / Frank Ferm Design. All rights reserved.
using Microsoft.EntityFrameworkCore;
using StitchTrack.Domain.Entities;

namespace StitchTrack.Infrastructure.Data;

/// <summary>
/// Handles database initialization and migrations.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Ensures the database is created and all migrations are applied.
    /// Safe to call on every app startup.
    /// </summary>
    public static async Task InitializeAsync(AppDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            await context.Database.MigrateAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            if (ex.InnerException != null)
                throw;
        }
    }
}
