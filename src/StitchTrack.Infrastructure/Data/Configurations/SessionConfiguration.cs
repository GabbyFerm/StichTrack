// Copyright (c) 2026 Gabriella Frank Ferm / Frank Ferm Design. All rights reserved.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StitchTrack.Domain.Entities;

namespace StitchTrack.Infrastructure.Data.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Sessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.StartedAt)
            .IsRequired();

        builder.Property(s => s.DurationSeconds)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.PrimaryCounterName)
            .HasMaxLength(100);

        // Index for queries
        builder.HasIndex(s => s.ProjectId);
        builder.HasIndex(s => s.StartedAt);
    }
}
