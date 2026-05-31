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

        // Relationship and cascade delete configured in ProjectCounterConfiguration
        // Index optimises per-counter undo queries (fetch latest change)
        builder.HasIndex(ch => new { ch.ProjectCounterId, ch.ChangedAt });
    }
}
