using Microsoft.EntityFrameworkCore;
using StitchTrack.Domain.Entities;
using StitchTrack.Domain.Interfaces;
using StitchTrack.Infrastructure.Data;

namespace StitchTrack.Infrastructure.Repositories;

/// <summary>
/// Repository for RowNote operations.
/// Follows the same patterns as ProjectRepository:
/// AsNoTracking for reads, ExecuteDeleteAsync for deletes.
/// </summary>
public class RowNoteRepository : IRowNoteRepository
{
    private readonly AppDbContext _context;

    public RowNoteRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<RowNote>> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.RowNotes
            .AsNoTracking()
            .Where(rn => rn.ProjectId == projectId)
            .OrderBy(rn => rn.RowNumber)       // show notes in row order
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task AddAsync(RowNote note)
    {
        ArgumentNullException.ThrowIfNull(note);
        await _context.RowNotes.AddAsync(note).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes directly via SQL — bypasses change tracker, no SaveChanges needed.
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        await _context.RowNotes
            .Where(rn => rn.Id == id)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);
    }

    public async Task<int> SaveChangesAsync()
    {
        var changes = await _context.SaveChangesAsync().ConfigureAwait(false);
        System.Diagnostics.Debug.WriteLine($"💾 RowNote: saved {changes} changes");
        return changes;
    }
}
