using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="EventVetoStep"/>.</summary>
public class EventVetoStepConfiguration : IEntityTypeConfiguration<EventVetoStep>
{
	#region Public Methods

	public void Configure(EntityTypeBuilder<EventVetoStep> builder)
	{
		builder.ToTable("EventVetoSteps");
		builder.HasKey(s => s.Id);

		builder.Property(s => s.Actor).HasConversion<string>().HasMaxLength(20);
		builder.Property(s => s.Action).HasConversion<string>().HasMaxLength(20);
		builder.Property(s => s.MapName).HasConversion<string>().HasMaxLength(20);

		// One step per position, one step per map within a single event's veto.
		builder.HasIndex(s => new { s.EventId, s.Order }).IsUnique();
		builder.HasIndex(s => new { s.EventId, s.MapName }).IsUnique();
	}

	#endregion
}
