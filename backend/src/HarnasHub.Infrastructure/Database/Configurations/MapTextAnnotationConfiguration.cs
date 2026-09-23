using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="MapTextAnnotation"/>.</summary>
public class MapTextAnnotationConfiguration : IEntityTypeConfiguration<MapTextAnnotation>
{
	#region Public Methods

	public void Configure(EntityTypeBuilder<MapTextAnnotation> builder)
	{
		builder.ToTable("MapTextAnnotations");
		builder.HasKey(a => a.Id);

		builder.Property(a => a.MapName).HasConversion<string>().HasMaxLength(20);
		builder.Property(a => a.Side).HasConversion<string>().HasMaxLength(20);
		builder.Property(a => a.Text).IsRequired().HasMaxLength(200);
		builder.Property(a => a.Color).IsRequired().HasMaxLength(7);

		builder.HasIndex(a => new { a.MapName, a.Side });
	}

	#endregion
}
