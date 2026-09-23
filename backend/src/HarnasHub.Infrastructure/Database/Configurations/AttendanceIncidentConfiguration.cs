using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="AttendanceIncident"/>.</summary>
public class AttendanceIncidentConfiguration : IEntityTypeConfiguration<AttendanceIncident>
{
	#region Public Methods

	public void Configure(EntityTypeBuilder<AttendanceIncident> builder)
	{
		builder.ToTable("AttendanceIncidents");
		builder.HasKey(i => i.Id);

		builder.Property(i => i.Note).HasMaxLength(300);

		builder.HasIndex(i => new { i.UserId, i.OccurredOn });
	}

	#endregion
}
