using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="TrainingMaterial"/>.</summary>
public class TrainingMaterialConfiguration : IEntityTypeConfiguration<TrainingMaterial>
{
    #region Public Methods

    public void Configure(EntityTypeBuilder<TrainingMaterial> builder)
    {
        builder.ToTable("TrainingMaterials");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title).IsRequired().HasMaxLength(150);
        builder.Property(m => m.Url).IsRequired().HasMaxLength(500);
        builder.Property(m => m.Category).HasMaxLength(50);
    }

    #endregion
}
