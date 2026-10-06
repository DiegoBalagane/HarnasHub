using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="HiddenOpponent"/> — one row per hidden opponent key.</summary>
public class HiddenOpponentConfiguration : IEntityTypeConfiguration<HiddenOpponent>
{
	#region Public Methods

	public void Configure(EntityTypeBuilder<HiddenOpponent> builder)
	{
		builder.ToTable("HiddenOpponents");
		builder.HasKey(h => h.Id);

		builder.Property(h => h.OpponentKey).IsRequired().HasMaxLength(100);
		builder.Property(h => h.DisplayName).IsRequired().HasMaxLength(100);

		builder.HasIndex(h => h.OpponentKey).IsUnique();
	}

	#endregion
}
