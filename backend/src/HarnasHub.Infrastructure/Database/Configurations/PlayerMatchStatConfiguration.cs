using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarnasHub.Infrastructure.Database.Configurations;

/// <summary>EF Core mapping for <see cref="PlayerMatchStat"/>.</summary>
public class PlayerMatchStatConfiguration : IEntityTypeConfiguration<PlayerMatchStat>
{
    #region Public Methods

    public void Configure(EntityTypeBuilder<PlayerMatchStat> builder)
    {
        builder.ToTable("PlayerMatchStats");
        builder.HasKey(s => s.Id);

        // One stat line per player per match.
        builder.HasIndex(s => new { s.MatchResultId, s.UserId }).IsUnique();
        builder.HasIndex(s => s.UserId);
    }

    #endregion
}
