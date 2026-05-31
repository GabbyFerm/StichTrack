// Copyright (c) 2026 Gabriella Frank Ferm / Frank Ferm Design. All rights reserved.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StitchTrack.Domain.Entities;

namespace StitchTrack.Infrastructure.Data.Configurations;

public class ProjectCounterConfiguration : IEntityTypeConfiguration<ProjectCounter>
{
    public void Configure(EntityTypeBuilder<ProjectCounter> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("ProjectCounters");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(c => c.CurrentCount)
               .IsRequired()
               .HasDefaultValue(0);

        builder.Property(c => c.SortOrder)
               .IsRequired();

        builder.Property(c => c.CreatedAt)
               .IsRequired();

        // Fast lookup for loading a project's counters in order
        builder.HasIndex(c => new { c.ProjectId, c.SortOrder });

        // Cascade: deleting a project deletes its counters
        builder.HasOne(c => c.Project)
               .WithMany(p => p.Counters)
               .HasForeignKey(c => c.ProjectId)
               .OnDelete(DeleteBehavior.Cascade);

        // Cascade: deleting a counter deletes its history entries
        builder.HasMany(c => c.CounterHistoryEntries)
               .WithOne(h => h.ProjectCounter)
               .HasForeignKey(h => h.ProjectCounterId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
