using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StitchTrack.Domain.Entities;

namespace StitchTrack.Infrastructure.Data.Configurations;

public class ReminderConfiguration : IEntityTypeConfiguration<Reminder>
{
    public void Configure(EntityTypeBuilder<Reminder> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Reminders");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.IntervalMinutes)
            .IsRequired();

        builder.Property(r => r.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(r => r.ProjectId);
    }
}
