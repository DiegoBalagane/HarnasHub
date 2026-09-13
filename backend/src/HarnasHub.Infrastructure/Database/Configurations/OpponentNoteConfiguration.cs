using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="OpponentNote"/>.</summary>
public class OpponentNoteConfiguration : IEntityTypeConfiguration<OpponentNote>
{
    #region Public Methods

    public void Configure(EntityTypeBuilder<OpponentNote> builder)
    {
        builder.ToTable("OpponentNotes");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.OpponentName).IsRequired().HasMaxLength(100);
        builder.Property(n => n.Content).IsRequired();
        builder.Property(n => n.MaterialUrl).HasMaxLength(500);

        builder.HasIndex(n => n.OpponentName);
    }

    #endregion
}
