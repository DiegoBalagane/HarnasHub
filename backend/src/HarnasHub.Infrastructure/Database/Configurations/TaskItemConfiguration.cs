using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="TaskItem"/>.</summary>
public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
	#region Public Methods

	public void Configure(EntityTypeBuilder<TaskItem> builder)
	{
		builder.ToTable("Tasks");
		builder.HasKey(t => t.Id);

		builder.Property(t => t.Title).IsRequired().HasMaxLength(150);
		builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);

		builder.HasIndex(t => t.AssignedToUserId);
	}

	#endregion
}
