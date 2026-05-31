// Copyright (c) 2026 Gabriella Frank Ferm / Frank Ferm Design. All rights reserved.
using Microsoft.EntityFrameworkCore;
using StitchTrack.Domain.Entities;
using StitchTrack.Domain.Interfaces;
using StitchTrack.Infrastructure.Data;

namespace StitchTrack.Infrastructure.Repositories;

public class ProjectCounterRepository : IProjectCounterRepository
{
    private readonly AppDbContext _context;

    public ProjectCounterRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<ProjectCounter>> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.ProjectCounters
            .AsNoTracking()
            .Where(c => c.ProjectId == projectId)
            .OrderBy(c => c.SortOrder)    // primary counter (0) always first
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task AddAsync(ProjectCounter counter)
    {
        ArgumentNullException.ThrowIfNull(counter);
        await _context.ProjectCounters.AddAsync(counter).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes a counter and all its history via ExecuteDeleteAsync.
    /// Cascade is also configured at the DB level as a safety net.
    /// No SaveChanges needed — executes immediately.
    /// </summary>
    public async Task DeleteAsync(Guid counterId)
    {
        // Delete history first (in case DB-level cascade isn't triggered by ExecuteDeleteAsync)
        await _context.CounterHistoryEntries
            .Where(h => h.ProjectCounterId == counterId)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);

        await _context.ProjectCounters
            .Where(c => c.Id == counterId)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);

    }

    /// <summary>
    /// Updates the counter's CurrentCount.
    /// When isPrimary is true, also syncs Project.CurrentCount so
    /// session tracking, export, and project cards stay accurate.
    /// All updates use ExecuteUpdateAsync — bypasses change tracker.
    /// </summary>
    public async Task UpdateCountAsync(
        Guid counterId, int newCount, bool isPrimary, Guid projectId)
    {
        await _context.ProjectCounters
            .Where(c => c.Id == counterId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.CurrentCount, newCount))
            .ConfigureAwait(false);

        if (isPrimary)
        {
            // Keep Project.CurrentCount in sync for sessions, export, project list
            await _context.Projects
                .Where(p => p.Id == projectId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.CurrentCount, newCount)
                    .SetProperty(p => p.UpdatedAt, DateTime.UtcNow))
                .ConfigureAwait(false);
        }

    }

    /// <summary>
    /// Renames a counter in place. Executes immediately — no SaveChanges needed.
    /// </summary>
    public async Task RenameAsync(Guid counterId, string newName)
    {
        ArgumentNullException.ThrowIfNull(newName);

        await _context.ProjectCounters
            .Where(c => c.Id == counterId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.Name, newName.Trim()))
            .ConfigureAwait(false);

    }

    public async Task<int> SaveChangesAsync()
    {
        var changes = await _context.SaveChangesAsync().ConfigureAwait(false);
        return changes;
    }
}
