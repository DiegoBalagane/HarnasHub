using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="PlayerAvailabilityDay"/>.</summary>
public class PlayerAvailabilityDayConfiguration : IEntityTypeConfiguration<PlayerAvailabilityDay>
{
	#region Public Methods

	public void Configure(EntityTypeBuilder<PlayerAvailabilityDay> builder)
	{
		builder.ToTable("PlayerAvailabilityDays");
		builder.HasKey(d => d.Id);

		builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(20);
		builder.Property(d => d.Note).HasMaxLength(300);

		// One declaration per player per day.
		builder.HasIndex(d => new { d.UserId, d.Date }).IsUnique();
	}

	#endregion
}
