using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="OpponentReportSnapshot"/> — one snapshot per opponent key, report as jsonb.</summary>
public class OpponentReportSnapshotConfiguration : IEntityTypeConfiguration<OpponentReportSnapshot>
{
	#region Public Methods

	public void Configure(EntityTypeBuilder<OpponentReportSnapshot> builder)
	{
		builder.ToTable("OpponentReportSnapshots");
		builder.HasKey(s => s.Id);

		builder.Property(s => s.OpponentKey).IsRequired().HasMaxLength(100);
		builder.Property(s => s.ReportJson).IsRequired().HasColumnType("jsonb");

		builder.HasIndex(s => s.OpponentKey).IsUnique();
	}

	#endregion
}
