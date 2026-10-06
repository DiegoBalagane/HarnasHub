#region Usings

using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

#endregion

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="BackgroundJob"/>.</summary>
public class BackgroundJobConfiguration : IEntityTypeConfiguration<BackgroundJob>
{
	#region Public Methods

	/// <summary>Maps the table, the JSON text columns and the indexes used by startup recovery and per-user lookups.</summary>
	public void Configure(EntityTypeBuilder<BackgroundJob> builder)
	{
		builder.ToTable("BackgroundJobs");
		builder.HasKey(j => j.Id);

		builder.Property(j => j.Kind).IsRequired().HasMaxLength(64);
		builder.Property(j => j.Status).IsRequired();
		builder.Property(j => j.Stage).HasMaxLength(200);
		// Plain text, not jsonb: demo player names may contain a NUL escape jsonb rejects, and the JSON is never queried.
		builder.Property(j => j.PayloadJson).IsRequired();
		builder.Property(j => j.ErrorMessage).HasMaxLength(1000);

		builder.HasIndex(j => j.Status);
		builder.HasIndex(j => new { j.RequestedByUserId, j.CreatedAtUtc });
	}

	#endregion
}
