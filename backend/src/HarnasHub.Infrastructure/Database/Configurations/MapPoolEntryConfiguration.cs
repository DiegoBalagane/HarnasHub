using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="MapPoolEntry"/> — at most one row per map (unique index on the map).</summary>
public class MapPoolEntryConfiguration : IEntityTypeConfiguration<MapPoolEntry>
{
	#region Public Methods

	public void Configure(EntityTypeBuilder<MapPoolEntry> builder)
	{
		builder.ToTable("MapPoolEntries");
		builder.HasKey(e => e.Id);

		builder.Property(e => e.MapName).HasConversion<string>().HasMaxLength(20);
		builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
		builder.Property(e => e.Note).HasMaxLength(300);

		builder.HasIndex(e => e.MapName).IsUnique();
	}

	#endregion
}
