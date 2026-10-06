#region Usings

using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

#endregion

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="OpponentDemoAnalysis"/>.</summary>
public class OpponentDemoAnalysisConfiguration : IEntityTypeConfiguration<OpponentDemoAnalysis>
{
	#region Public Methods

	/// <summary>Maps the table, array columns, the facts JSON and the per-opponent / per-FACEIT-map indexes.</summary>
	public void Configure(EntityTypeBuilder<OpponentDemoAnalysis> builder)
	{
		builder.ToTable("OpponentDemoAnalyses");
		builder.HasKey(a => a.Id);

		builder.Property(a => a.OpponentKey).IsRequired().HasMaxLength(100);
		builder.Property(a => a.MapName).HasConversion<string>().HasMaxLength(20);
		builder.Property(a => a.RawMapName).HasMaxLength(64);
		builder.Property(a => a.Source).HasConversion<string>().HasMaxLength(20);
		builder.Property(a => a.FaceitMatchId).HasMaxLength(64);
		builder.Property(a => a.TimelineObjectKey).IsRequired().HasMaxLength(300);
		builder.Property(a => a.TeamASteamIds).IsRequired();
		builder.Property(a => a.TeamBSteamIds).IsRequired();
		builder.Property(a => a.TeamANames).IsRequired();
		builder.Property(a => a.TeamBNames).IsRequired();
		builder.Property(a => a.OpponentSteamIds).IsRequired();

		builder.HasIndex(a => a.OpponentKey);
		builder.HasIndex(a => new { a.OpponentKey, a.FaceitMatchId, a.FaceitMapNumber });
	}

	#endregion
}
