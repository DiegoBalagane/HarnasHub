using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="FaceitPlayer"/>, keyed by the FACEIT player id.</summary>
public class FaceitPlayerConfiguration : IEntityTypeConfiguration<FaceitPlayer>
{
	#region Public Methods

	public void Configure(EntityTypeBuilder<FaceitPlayer> builder)
	{
		builder.ToTable("FaceitPlayers");
		builder.HasKey(p => p.Id);

		builder.Property(p => p.Id).HasMaxLength(64).ValueGeneratedNever();
		builder.Property(p => p.Nickname).IsRequired().HasMaxLength(64);
		builder.Property(p => p.SteamId64).HasMaxLength(20);
		builder.Property(p => p.MapStatsJson).HasColumnType("jsonb");

		builder.HasIndex(p => p.SteamId64);
	}

	#endregion
}
