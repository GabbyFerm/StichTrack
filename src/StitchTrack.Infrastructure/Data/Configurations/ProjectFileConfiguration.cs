using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StitchTrack.Domain.Entities;

namespace StitchTrack.Infrastructure.Data.Configurations;

public class ProjectFileConfiguration : IEntityTypeConfiguration<ProjectFile>
{
    public void Configure(EntityTypeBuilder<ProjectFile> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("ProjectFiles");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.FileType)
               .IsRequired();

        builder.Property(f => f.FileName)
               .IsRequired()
               .HasMaxLength(255);

        builder.Property(f => f.FilePath)
               .HasMaxLength(512);

        builder.Property(f => f.FileUrl)
               .HasMaxLength(512);

        builder.Property(f => f.ContentType)
               .HasMaxLength(100);

        builder.Property(f => f.FileSizeBytes)
               .IsRequired();

        builder.Property(f => f.UploadedAt)
               .IsRequired();

        // Index for fast lookups by project and type
        builder.HasIndex(f => new { f.ProjectId, f.FileType });

        builder.HasOne(f => f.Project)
               .WithMany(p => p.ProjectFiles)
               .HasForeignKey(f => f.ProjectId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
