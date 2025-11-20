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

        // Index for queries
        builder.HasIndex(s => s.ProjectId);
        builder.HasIndex(s => s.StartedAt);
    }
}
