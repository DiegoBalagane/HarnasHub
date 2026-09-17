using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="Tactic"/>.</summary>
public class TacticConfiguration : IEntityTypeConfiguration<Tactic>
{
	#region Public Methods

	public void Configure(EntityTypeBuilder<Tactic> builder)
	{
		builder.ToTable("Tactics");
		builder.HasKey(t => t.Id);

		builder.Property(t => t.MapName).HasConversion<string>().HasMaxLength(20);
		builder.Property(t => t.Side).HasConversion<string>().HasMaxLength(20);
		builder.Property(t => t.Economy).HasConversion<string>().HasMaxLength(20);
		builder.Property(t => t.Name).HasMaxLength(100);
		builder.Property(t => t.Note).HasMaxLength(500);

		builder.HasMany(t => t.Points)
			.WithOne()
			.HasForeignKey(p => p.TacticId)
			.OnDelete(DeleteBehavior.Cascade);
	}

	#endregion
}
