using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="MatchResult"/>.</summary>
public class MatchResultConfiguration : IEntityTypeConfiguration<MatchResult>
{
	#region Public Methods

	public void Configure(EntityTypeBuilder<MatchResult> builder)
	{
		builder.ToTable("MatchResults");
		builder.HasKey(m => m.Id);

		builder.Property(m => m.Opponent).IsRequired().HasMaxLength(100);
		builder.Property(m => m.MapName).HasMaxLength(50);
		builder.Property(m => m.DemoUrl).HasMaxLength(500);

		builder.HasIndex(m => m.PlayedAtUtc);
	}

	#endregion
}
