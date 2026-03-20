namespace StitchTrack.Domain.Interfaces;

using StitchTrack.Domain.Entities;

/// <summary>
/// Repository interface for Session entity.
/// Handles persistence of work sessions tied to projects.
/// </summary>
public interface ISessionRepository
{
    // ─── Phase 1 ────────────────────────────────────────────────

    /// <summary>
    /// Adds a new session record to the database context.
    /// Call SaveChangesAsync to persist.
    /// </summary>
    Task AddAsync(Session session);

    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync();

    // ─── Phase 2 / Stats page ───────────────────────────────────

    /// <summary>
    /// Returns all sessions for a given project, ordered newest first.
    /// Used for session history on SingleProjectPage.
    /// </summary>
    Task<IEnumerable<Session>> GetByProjectIdAsync(Guid projectId);

    /// <summary>
    /// Returns all sessions across all projects for the current user.
    /// Used for the Stats page overview.
    /// </summary>
    Task<IEnumerable<Session>> GetAllAsync();

    /// <summary>
    /// Returns sessions that started within the given date range.
    /// Used for Stats page filters (Today, This Week, This Month, All).
    /// </summary>
    Task<IEnumerable<Session>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);

    /// <summary>
    /// Returns the most recent N sessions across all projects.
    /// Used for "Recent Sessions" list on the Stats page.
    /// </summary>
    Task<IEnumerable<Session>> GetRecentAsync(int count = 10);

    /// <summary>
    /// Deletes all sessions belonging to a project.
    /// Called before hard-deleting a project.
    /// </summary>
    Task DeleteByProjectIdAsync(Guid projectId);
}
