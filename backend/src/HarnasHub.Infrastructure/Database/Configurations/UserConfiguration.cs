using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="User"/>.</summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
	#region Public Methods

	public void Configure(EntityTypeBuilder<User> builder)
	{
		builder.ToTable("Users");
		builder.HasKey(u => u.Id);

		builder.Property(u => u.DiscordId).IsRequired().HasMaxLength(32);
		builder.HasIndex(u => u.DiscordId).IsUnique();

		builder.Property(u => u.DisplayName).IsRequired().HasMaxLength(50);
		builder.Property(u => u.AvatarUrl).HasMaxLength(500);
		builder.Property(u => u.AccessLevel).HasConversion<string>().HasMaxLength(20);
		builder.Property(u => u.IsCoach).IsRequired().HasDefaultValue(false);
		builder.Property(u => u.TeamRole).HasConversion<string>().HasMaxLength(20);
		builder.Property(u => u.RosterSlot).HasConversion<string>().HasMaxLength(20);
		builder.Property(u => u.PinColor).HasConversion<string>().HasMaxLength(20);
		builder.Property(u => u.PinMark).HasMaxLength(4);
		builder.Property(u => u.InGameNickname).HasMaxLength(50);
	}

	#endregion
}
