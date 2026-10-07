using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="TeamInfoEntry"/>.</summary>
public class TeamInfoEntryConfiguration : IEntityTypeConfiguration<TeamInfoEntry>
{
	#region Public Methods

	public void Configure(EntityTypeBuilder<TeamInfoEntry> builder)
	{
		builder.ToTable("TeamInfoEntries");
		builder.HasKey(e => e.Id);

		builder.Property(e => e.Category).IsRequired().HasMaxLength(50);
		builder.Property(e => e.Title).IsRequired().HasMaxLength(100);
		builder.Property(e => e.Value).IsRequired().HasMaxLength(2000);

		builder.HasIndex(e => new { e.Category, e.SortOrder });
	}

	#endregion
}
