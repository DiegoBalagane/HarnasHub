using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="MapPositionAssignment"/>.</summary>
public class MapPositionAssignmentConfiguration : IEntityTypeConfiguration<MapPositionAssignment>
{
	#region Public Methods

	public void Configure(EntityTypeBuilder<MapPositionAssignment> builder)
	{
		builder.ToTable("MapPositionAssignments");
		builder.HasKey(p => p.Id);

		builder.Property(p => p.MapName).HasConversion<string>().HasMaxLength(20);
		builder.Property(p => p.Side).HasConversion<string>().HasMaxLength(20);
		builder.Property(p => p.Label).HasMaxLength(50);
		builder.Property(p => p.Note).HasMaxLength(300);

		// A player holds at most one spot per map/side, so the upsert key is also the uniqueness guarantee.
		builder.HasIndex(p => new { p.MapName, p.Side, p.UserId }).IsUnique();
	}

	#endregion
}
