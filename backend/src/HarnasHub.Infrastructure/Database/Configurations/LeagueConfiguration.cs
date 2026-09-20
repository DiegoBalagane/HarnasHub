using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="League"/>.</summary>
public class LeagueConfiguration : IEntityTypeConfiguration<League>
{
	#region Public Methods

	public void Configure(EntityTypeBuilder<League> builder)
	{
		builder.ToTable("Leagues");
		builder.HasKey(l => l.Id);

		builder.Property(l => l.Name).IsRequired().HasMaxLength(100);
		builder.Property(l => l.Season).IsRequired().HasMaxLength(50);
		builder.Property(l => l.Type).HasConversion<string>().HasMaxLength(20);
	}

	#endregion
}
