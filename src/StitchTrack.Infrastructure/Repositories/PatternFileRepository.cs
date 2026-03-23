using Microsoft.EntityFrameworkCore;
using StitchTrack.Domain.Entities;
using StitchTrack.Domain.Interfaces;
using StitchTrack.Infrastructure.Data;

namespace StitchTrack.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IPatternFileRepository.
/// </summary>
public class PatternFileRepository : IPatternFileRepository
{
    private readonly AppDbContext _context;

    public PatternFileRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAsync(PatternFile patternFile)
    {
        ArgumentNullException.ThrowIfNull(patternFile);
        await _context.PatternFiles.AddAsync(patternFile).ConfigureAwait(false);
        System.Diagnostics.Debug.WriteLine($"📎 Pattern file added: {patternFile.FileName}");
    }

    public async Task<IEnumerable<PatternFile>> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.PatternFiles
            .Where(f => f.ProjectId == projectId)
            .OrderByDescending(f => f.UploadedAt)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task DeleteAsync(Guid id)
    {
        var file = await _context.PatternFiles
            .FirstOrDefaultAsync(f => f.Id == id)
            .ConfigureAwait(false);

        if (file != null)
        {
            _context.PatternFiles.Remove(file);
            System.Diagnostics.Debug.WriteLine($"🗑️ Pattern file deleted: {file.FileName}");
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        var changes = await _context.SaveChangesAsync().ConfigureAwait(false);
        System.Diagnostics.Debug.WriteLine($"💾 Saved {changes} pattern file changes");
        return changes;
    }
}
