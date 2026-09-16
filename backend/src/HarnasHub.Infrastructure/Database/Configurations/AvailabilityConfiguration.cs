using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="Availability"/>.</summary>
public class AvailabilityConfiguration : IEntityTypeConfiguration<Availability>
{
	#region Public Methods

	public void Configure(EntityTypeBuilder<Availability> builder)
	{
		builder.ToTable("Availabilities");
		builder.HasKey(a => a.Id);

		builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);

		// One availability declaration per player per event.
		builder.HasIndex(a => new { a.EventId, a.UserId }).IsUnique();
	}

	#endregion
}
