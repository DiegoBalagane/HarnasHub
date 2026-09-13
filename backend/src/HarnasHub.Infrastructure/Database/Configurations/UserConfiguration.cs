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
        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
    }

    #endregion
}
