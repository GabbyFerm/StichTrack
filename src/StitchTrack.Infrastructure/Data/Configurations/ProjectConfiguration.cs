using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StitchTrack.Domain.Entities;

namespace StitchTrack.Infrastructure.Data.Configurations;

/// <summary>
/// Configures the Project entity mapping to database table
/// </summary>
public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Table name
        builder.ToTable("Projects");

        // Primary key
        builder.HasKey(p => p.Id);

        // Required fields
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.CurrentCount)
            .IsRequired()
            .HasDefaultValue(0);

        // Optional fields with max lengths
        builder.Property(p => p.ColorHex)
            .HasMaxLength(9);

        builder.Property(p => p.Notes)
            .HasMaxLength(4000);

        builder.Property(p => p.ImagePath)
            .HasMaxLength(512);

        builder.Property(p => p.ImageUrl)
            .HasMaxLength(512);

        // Timestamps
        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired();

        // 🆕 Cloud sync fields (Phase 3)
        builder.Property(p => p.LastSyncedAt);

        builder.Property(p => p.CloudFileId)
            .HasMaxLength(255);

        builder.Property(p => p.SyncVersion)
            .IsRequired()
            .HasDefaultValue(0);

        // Foreign key to User 
        builder.HasOne(p => p.User)
            .WithMany(u => u.Projects)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Relationships - one-to-many
        builder.HasMany(p => p.CounterHistoryEntries)
            .WithOne(ch => ch.Project)
            .HasForeignKey(ch => ch.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Sessions)
            .WithOne(s => s.Project)
            .HasForeignKey(s => s.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.RowNotes)
            .WithOne(rn => rn.Project)
            .HasForeignKey(rn => rn.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Reminders)
            .WithOne(r => r.Project)
            .HasForeignKey(r => r.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.PatternFiles)
            .WithOne(pf => pf.Project)
            .HasForeignKey(pf => pf.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for performance
        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.UpdatedAt);
        builder.HasIndex(p => p.IsArchived);
    }
}
