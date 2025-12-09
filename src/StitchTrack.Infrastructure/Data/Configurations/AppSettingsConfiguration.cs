using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StitchTrack.Domain.Entities;

namespace StitchTrack.Infrastructure.Data.Configurations;

/// <summary>
/// Configures the AppSettings entity mapping to database table.
/// Single-row table with well-known GUID.
/// </summary>
public class AppSettingsConfiguration : IEntityTypeConfiguration<AppSettings>
{
    public void Configure(EntityTypeBuilder<AppSettings> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Table name
        builder.ToTable("AppSettings");

        // Primary key
        builder.HasKey(s => s.Id);

        // Required fields
        builder.Property(s => s.IsFirstRun)
            .IsRequired();

        builder.Property(s => s.Theme)
            .IsRequired()
            .HasMaxLength(10); // "Light", "Dark", "Auto"

        builder.Property(s => s.HapticFeedbackEnabled)
            .IsRequired();

        builder.Property(s => s.ProjectCreationCount)
            .IsRequired();

        // Optional fields
        builder.Property(s => s.FirstRunCompletedAt);

        builder.Property(s => s.SyncEnabled)
            .IsRequired();

        builder.Property(s => s.SyncProvider)
            .HasMaxLength(50); // "iCloud", "GoogleDrive", "Dropbox"

        builder.Property(s => s.LastSuccessfulSync);

        // Seed data: Insert default settings row
        builder.HasData(new
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            IsFirstRun = true,
            Theme = "Auto",
            HapticFeedbackEnabled = true,
            SyncEnabled = false,
            ProjectCreationCount = 0
        });
    }
}
