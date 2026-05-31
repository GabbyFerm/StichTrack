// Copyright (c) 2026 Gabriella Frank Ferm / Frank Ferm Design. All rights reserved.
using Microsoft.EntityFrameworkCore;
using StitchTrack.Domain.Entities;
using StitchTrack.Domain.Interfaces;
using StitchTrack.Infrastructure.Data;

namespace StitchTrack.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of ISessionRepository.
/// </summary>
public class SessionRepository : ISessionRepository
{
    private readonly AppDbContext _context;

    public SessionRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAsync(Session session)
    {
        ArgumentNullException.ThrowIfNull(session);
        await _context.Sessions.AddAsync(session).ConfigureAwait(false);
    }

    /// <summary>
    /// Saves all pending changes to the database.
    /// Special handling: Cleans up stale CounterHistory entries in the change tracker
    /// to prevent spurious UPDATE attempts on entities from previous page loads.
    /// This is necessary because ProjectCounterPage loads CounterHistory via GetByIdAsync
    /// but doesn't intend to modify it — the detach prevents accidental state tracking issues.
    /// </summary>
    public async Task<int> SaveChangesAsync()
    {
        // Detach stale CounterHistory entries tracked from previous page loads
        // to prevent EF trying to UPDATE rows that were never inserted
        var staleEntries = _context.ChangeTracker
            .Entries<CounterHistory>()
            .Where(e => e.State == EntityState.Modified)
            .ToList();

        foreach (var entry in staleEntries)
            entry.State = EntityState.Detached;

        var changes = await _context.SaveChangesAsync().ConfigureAwait(false);
        return changes;
    }

    public async Task<IEnumerable<Session>> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.Sessions
            .AsNoTracking()
            .Where(s => s.ProjectId == projectId)
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<Session>> GetAllAsync()
    {
        return await _context.Sessions
            .AsNoTracking()
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Includes Project navigation property so the Stats page can show project names.
    /// </summary>
    public async Task<IEnumerable<Session>> GetAllWithProjectAsync()
    {
        return await _context.Sessions
            .AsNoTracking()
            .Include(s => s.Project)
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<Session>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        return await _context.Sessions
            .AsNoTracking()
            .Where(s => s.StartedAt >= fromDate && s.StartedAt <= toDate)
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<Session>> GetRecentAsync(int count = 10)
    {
        return await _context.Sessions
            .AsNoTracking()
            .OrderByDescending(s => s.StartedAt)
            .Take(count)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task DeleteByProjectIdAsync(Guid projectId)
    {
        var sessions = await _context.Sessions
            .Where(s => s.ProjectId == projectId)
            .ToListAsync()
            .ConfigureAwait(false);

        _context.Sessions.RemoveRange(sessions);
    }
}
