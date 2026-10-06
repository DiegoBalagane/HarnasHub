using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="EventGamePlanItem"/> — each tactic/board at most once per event's plan.</summary>
public class EventGamePlanItemConfiguration : IEntityTypeConfiguration<EventGamePlanItem>
{
	#region Public Methods

	public void Configure(EntityTypeBuilder<EventGamePlanItem> builder)
	{
		builder.ToTable("EventGamePlanItems");
		builder.HasKey(i => i.Id);

		builder.Property(i => i.Kind).HasConversion<string>().HasMaxLength(20);

		builder.HasIndex(i => new { i.EventId, i.Kind, i.TargetId }).IsUnique();
	}

	#endregion
}
