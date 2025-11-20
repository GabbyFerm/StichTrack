using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StitchTrack.Domain.Entities;

namespace StitchTrack.Infrastructure.Data.Configurations;

public class PatternFileConfiguration : IEntityTypeConfiguration<PatternFile>
{
    public void Configure(EntityTypeBuilder<PatternFile> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("PatternFiles");

        builder.HasKey(pf => pf.Id);

        builder.Property(pf => pf.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(pf => pf.FilePath)
            .HasMaxLength(512);

        builder.Property(pf => pf.FileUrl)
            .HasMaxLength(512);

        builder.Property(pf => pf.ContentType)
            .HasMaxLength(100);

        builder.Property(pf => pf.FileSizeBytes)
            .IsRequired();

        builder.Property(pf => pf.UploadedAt)
            .IsRequired();

        builder.HasIndex(pf => pf.ProjectId);
    }
}
