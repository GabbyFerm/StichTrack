using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StitchTrack.Domain.Entities;

namespace StitchTrack.Infrastructure.Data.Configurations;

public class RowNoteConfiguration : IEntityTypeConfiguration<RowNote>
{
    public void Configure(EntityTypeBuilder<RowNote> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("RowNotes");

        builder.HasKey(rn => rn.Id);

        builder.Property(rn => rn.RowNumber)
            .IsRequired();

        builder.Property(rn => rn.NoteText)
            .HasMaxLength(1000);

        builder.Property(rn => rn.CreatedAt)
            .IsRequired();

        // Index for finding notes by row number
        builder.HasIndex(rn => new { rn.ProjectId, rn.RowNumber });
    }
}
