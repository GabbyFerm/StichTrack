using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StitchTrack.Domain.Entities;

namespace StitchTrack.Infrastructure.Data.Configurations;

public class CounterHistoryConfiguration : IEntityTypeConfiguration<CounterHistory>
{
    public void Configure(EntityTypeBuilder<CounterHistory> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("CounterHistory");

        builder.HasKey(ch => ch.Id);

        builder.Property(ch => ch.OldValue)
            .IsRequired();

        builder.Property(ch => ch.NewValue)
            .IsRequired();

        builder.Property(ch => ch.ChangedAt)
            .IsRequired();

        // Foreign key configured in ProjectConfiguration
        // Index for undo queries (get latest by date)
        builder.HasIndex(ch => new { ch.ProjectId, ch.ChangedAt });
    }
}
