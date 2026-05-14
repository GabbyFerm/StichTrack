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

        // Foreign key and cascade delete configured in ProjectConfiguration via HasMany().WithOne()
        // This index optimizes undo queries that fetch the latest change by date
        builder.HasIndex(ch => new { ch.ProjectId, ch.ChangedAt });
    }
}
