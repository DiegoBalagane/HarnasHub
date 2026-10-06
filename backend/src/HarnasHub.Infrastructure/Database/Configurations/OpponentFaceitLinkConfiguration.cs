using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="OpponentFaceitLink"/> — one link per opponent key, player ids as a text array.</summary>
public class OpponentFaceitLinkConfiguration : IEntityTypeConfiguration<OpponentFaceitLink>
{
	#region Public Methods

	public void Configure(EntityTypeBuilder<OpponentFaceitLink> builder)
	{
		builder.ToTable("OpponentFaceitLinks");
		builder.HasKey(l => l.Id);

		builder.Property(l => l.OpponentKey).IsRequired().HasMaxLength(100);
		builder.Property(l => l.DisplayName).IsRequired().HasMaxLength(100);
		builder.Property(l => l.FaceitTeamId).HasMaxLength(64);
		builder.Property(l => l.PlayerIds).IsRequired();

		builder.HasIndex(l => l.OpponentKey).IsUnique();
	}

	#endregion
}
