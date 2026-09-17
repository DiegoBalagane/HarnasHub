using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="TacticPoint"/>.</summary>
public class TacticPointConfiguration : IEntityTypeConfiguration<TacticPoint>
{
	#region Public Methods

	public void Configure(EntityTypeBuilder<TacticPoint> builder)
	{
		builder.ToTable("TacticPoints");
		builder.HasKey(p => p.Id);

		builder.Property(p => p.Description).HasMaxLength(300);
	}

	#endregion
}
