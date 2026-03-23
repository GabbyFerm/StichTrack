using Microsoft.EntityFrameworkCore;
using StitchTrack.Domain.Entities;
using StitchTrack.Domain.Interfaces;
using StitchTrack.Infrastructure.Data;

namespace StitchTrack.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Project entity.
/// Handles database operations for projects using Entity Framework Core.
/// </summary>
public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;

    public ProjectRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAsync(Project project)
    {
        ArgumentNullException.ThrowIfNull(project);

        await _context.Projects.AddAsync(project).ConfigureAwait(false);
        System.Diagnostics.Debug.WriteLine($"📝 Project added to context: {project.Name} (ID: {project.Id})");
    }

    public async Task<Project?> GetByIdAsync(Guid id)
    {
        return await _context.Projects
            .AsNoTracking()  
            .Include(p => p.CounterHistoryEntries)
            .Include(p => p.Sessions)
            .Include(p => p.PatternFiles)
            .FirstOrDefaultAsync(p => p.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<Project?> GetByIdWithoutHistoryAsync(Guid id)
    {
        return await _context.Projects
            .Include(p => p.Sessions)
            .FirstOrDefaultAsync(p => p.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<Project>> GetActiveProjectsAsync(Guid? userId = null)
    {
        var query = _context.Projects
        .AsNoTracking()
        .Where(p => !p.IsArchived);

        if (userId.HasValue)
        {
            query = query.Where(p => p.UserId == userId.Value);
        }
        else
        {
            // Guest mode: only projects without a user
            query = query.Where(p => p.UserId == null);
        }

        return await query
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<Project>> GetArchivedProjectsAsync(Guid? userId = null)
    {
        var query = _context.Projects
        .AsNoTracking()
        .Where(p => p.IsArchived);

        if (userId.HasValue)
        {
            query = query.Where(p => p.UserId == userId.Value);
        }
        else
        {
            query = query.Where(p => p.UserId == null);
        }

        return await query
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task UpdateAsync(Project project)
    {
        ArgumentNullException.ThrowIfNull(project);

        // If the project was loaded via GetByIdAsync in this same DbContext instance,
        // EF Core is already tracking it and knows what changed.
        // Calling Update() would override child entity states (e.g. Added → Modified)
        // causing INSERT to become UPDATE on rows that don't exist yet.
        var entry = _context.Entry(project);
        if (entry.State == EntityState.Detached)
        {
            // Only attach and mark modified if not already tracked
            _context.Projects.Update(project);
        }

        System.Diagnostics.Debug.WriteLine($"🔄 Project updated: {project.Name}");
        await Task.CompletedTask.ConfigureAwait(false);
    }

    public async Task UpdateCountAsync(Guid projectId, int newCount, DateTime updatedAt)
    {
        await _context.Projects
            .Where(p => p.Id == projectId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.CurrentCount, newCount)
                .SetProperty(p => p.UpdatedAt, updatedAt))
            .ConfigureAwait(false);

        System.Diagnostics.Debug.WriteLine($"💾 Count updated to {newCount}");
    }

    /// <summary>
    /// Soft delete — marks the project as archived so it can be recovered.
    /// </summary>
    public async Task ArchiveAsync(Guid id)
    {
        var project = await GetByIdAsync(id).ConfigureAwait(false);
        if (project != null)
        {
            project.ArchiveProject();
            System.Diagnostics.Debug.WriteLine($"📦 Project soft-deleted (archived): {project.Name}");
        }
    }

    /// <summary>
    /// Hard delete — permanently removes the project from the database.
    /// Does nothing silently if the project is not found.
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        // Lightweight fetch — no Includes needed, just the row to remove
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == id)
            .ConfigureAwait(false);

        if (project != null)
        {
            _context.Projects.Remove(project);
            System.Diagnostics.Debug.WriteLine($"🗑️ Project hard-deleted: {project.Name}");
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        var changes = await _context.SaveChangesAsync().ConfigureAwait(false);
        System.Diagnostics.Debug.WriteLine($"💾 Saved {changes} changes to database");
        return changes;
    }
}
