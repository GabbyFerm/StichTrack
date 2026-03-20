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
        System.Diagnostics.Debug.WriteLine($"📝 Session added to context: {session.Id}");
    }

    public async Task<int> SaveChangesAsync()
    {
        // Detach stale CounterHistory entries tracked from previous page loads
        // to prevent EF trying to UPDATE rows that were never inserted in this session
        var staleHistoryEntries = _context.ChangeTracker
            .Entries<CounterHistory>()
            .Where(e => e.State == EntityState.Modified)
            .ToList();

        foreach (var entry in staleHistoryEntries)
            entry.State = EntityState.Detached;

        var changes = await _context.SaveChangesAsync().ConfigureAwait(false);
        System.Diagnostics.Debug.WriteLine($"💾 Saved {changes} session changes");
        return changes;
    }

    /// <summary>
    /// Returns all sessions for a project, newest first.
    /// </summary>
    public async Task<IEnumerable<Session>> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.Sessions
            .Where(s => s.ProjectId == projectId)
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Returns all sessions across all projects.
    /// </summary>
    public async Task<IEnumerable<Session>> GetAllAsync()
    {
        return await _context.Sessions
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Returns sessions within a date range for Stats page filters.
    /// </summary>
    public async Task<IEnumerable<Session>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        return await _context.Sessions
            .Where(s => s.StartedAt >= fromDate && s.StartedAt <= toDate)
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the most recent N sessions for the Recent Sessions list.
    /// </summary>
    public async Task<IEnumerable<Session>> GetRecentAsync(int count = 10)
    {
        return await _context.Sessions
            .OrderByDescending(s => s.StartedAt)
            .Take(count)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes all sessions for a project — called before hard-deleting a project.
    /// </summary>
    public async Task DeleteByProjectIdAsync(Guid projectId)
    {
        var sessions = await _context.Sessions
            .Where(s => s.ProjectId == projectId)
            .ToListAsync()
            .ConfigureAwait(false);

        _context.Sessions.RemoveRange(sessions);
        System.Diagnostics.Debug.WriteLine($"🗑️ Deleted {sessions.Count} sessions for project {projectId}");
    }
}
