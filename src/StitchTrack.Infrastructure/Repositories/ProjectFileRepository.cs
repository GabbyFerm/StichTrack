using Microsoft.EntityFrameworkCore;
using StitchTrack.Domain.Entities;
using StitchTrack.Domain.Interfaces;
using StitchTrack.Infrastructure.Data;

namespace StitchTrack.Infrastructure.Repositories;

public class ProjectFileRepository : IProjectFileRepository
{
    private readonly AppDbContext _context;

    public ProjectFileRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAsync(ProjectFile file)
    {
        ArgumentNullException.ThrowIfNull(file);
        await _context.ProjectFiles.AddAsync(file).ConfigureAwait(false);
        System.Diagnostics.Debug.WriteLine($"📎 Project file added: {file.FileName} ({file.FileType})");
    }

    public async Task<IEnumerable<ProjectFile>> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.ProjectFiles
            .AsNoTracking()
            .Where(f => f.ProjectId == projectId)
            .OrderByDescending(f => f.UploadedAt)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<ProjectFile>> GetByProjectIdAndTypeAsync(
        Guid projectId, ProjectFileType fileType)
    {
        return await _context.ProjectFiles
            .AsNoTracking()
            .Where(f => f.ProjectId == projectId && f.FileType == fileType)
            .OrderByDescending(f => f.UploadedAt)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Uses ExecuteDeleteAsync to delete directly via SQL — bypasses change tracker entirely.
    /// No SaveChangesAsync needed; executes immediately as a separate database operation.
    /// This differs from AddAsync (which requires SaveChangesAsync) and is only part of a
    /// transaction if the caller has explicitly opened one.
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        await _context.ProjectFiles
            .Where(f => f.Id == id)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);

        System.Diagnostics.Debug.WriteLine($"🗑️ Project file deleted: {id}");
    }

    public async Task<int> SaveChangesAsync()
    {
        var changes = await _context.SaveChangesAsync().ConfigureAwait(false);
        System.Diagnostics.Debug.WriteLine($"💾 Saved {changes} project file changes");
        return changes;
    }
}
