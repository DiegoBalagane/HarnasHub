using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="UserSecondaryTeamRole"/>.</summary>
public class UserSecondaryTeamRoleConfiguration : IEntityTypeConfiguration<UserSecondaryTeamRole>
{
	#region Public Methods

	public void Configure(EntityTypeBuilder<UserSecondaryTeamRole> builder)
	{
		builder.ToTable("UserSecondaryTeamRoles");
		builder.HasKey(r => r.Id);

		builder.Property(r => r.TeamRole).HasConversion<string>().HasMaxLength(20);

		// A player holds each secondary role at most once.
		builder.HasIndex(r => new { r.UserId, r.TeamRole }).IsUnique();
	}

	#endregion
}
