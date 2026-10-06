using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="EventGamePlan"/> — at most one plan per event.</summary>
public class EventGamePlanConfiguration : IEntityTypeConfiguration<EventGamePlan>
{
	#region Public Methods

	public void Configure(EntityTypeBuilder<EventGamePlan> builder)
	{
		builder.ToTable("EventGamePlans");
		builder.HasKey(p => p.Id);

		builder.Property(p => p.Notes).HasMaxLength(4000);

		builder.HasIndex(p => p.EventId).IsUnique();
	}

	#endregion
}
