#region Usings

using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

#endregion

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="MatchDemoAnalysis"/>.</summary>
public class MatchDemoAnalysisConfiguration : IEntityTypeConfiguration<MatchDemoAnalysis>
{
	#region Public Methods

	/// <summary>Maps the table, the unique per-match index and the SteamID64 array column.</summary>
	public void Configure(EntityTypeBuilder<MatchDemoAnalysis> builder)
	{
		builder.ToTable("MatchDemoAnalyses");
		builder.HasKey(a => a.Id);

		builder.Property(a => a.ObjectKey).IsRequired().HasMaxLength(300);
		builder.Property(a => a.OurTeamSteamIds).IsRequired();

		builder.HasIndex(a => a.MatchResultId).IsUnique();
	}

	#endregion
}
