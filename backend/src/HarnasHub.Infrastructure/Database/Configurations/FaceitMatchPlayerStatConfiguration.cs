using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="FaceitMatchPlayerStat"/>; lines are removed together with their match map.</summary>
public class FaceitMatchPlayerStatConfiguration : IEntityTypeConfiguration<FaceitMatchPlayerStat>
{
	#region Public Methods

	public void Configure(EntityTypeBuilder<FaceitMatchPlayerStat> builder)
	{
		builder.ToTable("FaceitMatchPlayerStats");
		builder.HasKey(s => s.Id);

		builder.Property(s => s.PlayerId).IsRequired().HasMaxLength(64);
		builder.Property(s => s.Nickname).IsRequired().HasMaxLength(64);

		builder.HasOne<FaceitMatch>()
			.WithMany()
			.HasForeignKey(s => s.MatchId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasIndex(s => new { s.MatchId, s.PlayerId }).IsUnique();
		builder.HasIndex(s => s.PlayerId);
	}

	#endregion
}
