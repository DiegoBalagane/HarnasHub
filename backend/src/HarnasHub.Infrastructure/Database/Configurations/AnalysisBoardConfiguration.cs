using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="AnalysisBoard"/>.</summary>
public class AnalysisBoardConfiguration : IEntityTypeConfiguration<AnalysisBoard>
{
	#region Public Methods

	public void Configure(EntityTypeBuilder<AnalysisBoard> builder)
	{
		builder.ToTable("AnalysisBoards");
		builder.HasKey(b => b.Id);

		builder.Property(b => b.MapName).HasConversion<string>().HasMaxLength(20);
		builder.Property(b => b.Title).IsRequired().HasMaxLength(150);
		builder.Property(b => b.BackgroundImageObjectKey).HasMaxLength(300);
		builder.Property(b => b.StrokesJson).IsRequired();

		builder.HasIndex(b => b.MapName);
	}

	#endregion
}
