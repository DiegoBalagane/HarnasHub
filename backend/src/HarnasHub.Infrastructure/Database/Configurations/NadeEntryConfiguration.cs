using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="NadeEntry"/>.</summary>
public class NadeEntryConfiguration : IEntityTypeConfiguration<NadeEntry>
{
	#region Public Methods

	public void Configure(EntityTypeBuilder<NadeEntry> builder)
	{
		builder.ToTable("NadeEntries");
		builder.HasKey(n => n.Id);

		builder.Property(n => n.MapName).HasConversion<string>().HasMaxLength(20);
		builder.Property(n => n.Type).HasConversion<string>().HasMaxLength(10);
		builder.Property(n => n.Title).IsRequired().HasMaxLength(100);
		builder.Property(n => n.YoutubeUrl).HasMaxLength(500);

		builder.HasIndex(n => new { n.MapName, n.Type });
	}

	#endregion
}
