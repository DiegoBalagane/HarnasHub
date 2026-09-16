using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="Vacation"/>.</summary>
public class VacationConfiguration : IEntityTypeConfiguration<Vacation>
{
	#region Public Methods

	public void Configure(EntityTypeBuilder<Vacation> builder)
	{
		builder.ToTable("Vacations");
		builder.HasKey(v => v.Id);

		builder.Property(v => v.Reason).HasMaxLength(300);

		builder.HasIndex(v => new { v.UserId, v.StartDate });
	}

	#endregion
}
