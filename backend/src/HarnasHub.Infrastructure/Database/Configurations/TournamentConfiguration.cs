using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="Tournament"/>.</summary>
public class TournamentConfiguration : IEntityTypeConfiguration<Tournament>
{
	#region Public Methods

	public void Configure(EntityTypeBuilder<Tournament> builder)
	{
		builder.ToTable("Tournaments");
		builder.HasKey(t => t.Id);

		builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
	}

	#endregion
}
