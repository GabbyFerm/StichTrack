using Microsoft.EntityFrameworkCore;
using StitchTrack.Domain.Entities;

namespace StitchTrack.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<CounterHistory> CounterHistoryEntries { get; set; } = null!;
    public DbSet<Session> Sessions { get; set; } = null!;
    public DbSet<RowNote> RowNotes { get; set; } = null!;
    public DbSet<Reminder> Reminders { get; set; } = null!;
    public DbSet<PatternFile> PatternFiles { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
