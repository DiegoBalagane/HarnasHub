using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="FaceitMatch"/> — one row per map, rosters as text arrays.</summary>
public class FaceitMatchConfiguration : IEntityTypeConfiguration<FaceitMatch>
{
	#region Public Methods

	public void Configure(EntityTypeBuilder<FaceitMatch> builder)
	{
		builder.ToTable("FaceitMatches");
		builder.HasKey(m => m.Id);

		builder.Property(m => m.FaceitMatchId).IsRequired().HasMaxLength(64);
		builder.Property(m => m.MapName).HasMaxLength(40);
		builder.Property(m => m.CompetitionType).HasMaxLength(40);
		builder.Property(m => m.CompetitionName).HasMaxLength(200);
		builder.Property(m => m.Team1Name).HasMaxLength(100);
		builder.Property(m => m.Team2Name).HasMaxLength(100);
		builder.Property(m => m.Team1PlayerIds).IsRequired();
		builder.Property(m => m.Team2PlayerIds).IsRequired();

		builder.HasIndex(m => new { m.FaceitMatchId, m.MapNumber }).IsUnique();
		builder.HasIndex(m => m.PlayedAtUtc);
	}

	#endregion
}
