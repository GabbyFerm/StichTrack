using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StitchTrack.Domain.Entities;

namespace StitchTrack.Infrastructure.Data.Configurations;

public class AppSettingsConfiguration : IEntityTypeConfiguration<AppSettings>
{
    public void Configure(EntityTypeBuilder<AppSettings> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("AppSettings");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.IsFirstRun)
            .IsRequired();

        builder.Property(s => s.Theme)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(s => s.SyncProvider)
            .HasMaxLength(50);

        builder.Property(s => s.HapticFeedbackEnabled)
            .IsRequired();

        builder.Property(s => s.ProjectCreationCount)
            .IsRequired();
    }
}
