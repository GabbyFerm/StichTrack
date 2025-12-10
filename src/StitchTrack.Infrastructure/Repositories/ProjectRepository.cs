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
            .Include(p => p.CounterHistoryEntries)
            .Include(p => p.Sessions)
            .FirstOrDefaultAsync(p => p.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<Project>> GetActiveProjectsAsync(Guid? userId = null)
    {
        var query = _context.Projects
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

        _context.Projects.Update(project);
        System.Diagnostics.Debug.WriteLine($"🔄 Project updated: {project.Name}");

        await Task.CompletedTask.ConfigureAwait(false);
    }

    public async Task DeleteAsync(Guid id)
    {
        var project = await GetByIdAsync(id).ConfigureAwait(false);
        if (project != null)
        {
            project.ArchiveProject();
            System.Diagnostics.Debug.WriteLine($"🗑️ Project archived: {project.Name}");
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        var changes = await _context.SaveChangesAsync().ConfigureAwait(false);
        System.Diagnostics.Debug.WriteLine($"💾 Saved {changes} changes to database");
        return changes;
    }
}
