using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Stats.Shared;
using HarnasHub.Core.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Stats.AddPlayerStat;

/// <summary>Handles <see cref="AddPlayerStatCommand"/> by persisting the player's stat line for the match.</summary>
public class AddPlayerStatHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier)
    : IRequestHandler<AddPlayerStatCommand, ErrorOr<PlayerMatchStatDto>>
{
    #region Public Methods

    public async Task<ErrorOr<PlayerMatchStatDto>> Handle(AddPlayerStatCommand request, CancellationToken cancellationToken)
    {
        var matchExists = await dbContext.MatchResults.AnyAsync(m => m.Id == request.MatchResultId, cancellationToken);

        if (!matchExists)
        {
            return StatsErrors.MatchNotFound;
        }

        var player = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (player is null)
        {
            return StatsErrors.PlayerNotFound;
        }

        var alreadyExists = await dbContext.PlayerMatchStats
            .AnyAsync(s => s.MatchResultId == request.MatchResultId && s.UserId == request.UserId, cancellationToken);

        if (alreadyExists)
        {
            return StatsErrors.StatAlreadyExists;
        }

        var stat = new PlayerMatchStat
        {
            Id = Guid.NewGuid(),
            MatchResultId = request.MatchResultId,
            UserId = request.UserId,
            Kills = request.Kills,
            Deaths = request.Deaths,
            Assists = request.Assists,
            Adr = request.Adr,
            HeadshotPercentage = request.HeadshotPercentage,
            Rating = request.Rating,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.PlayerMatchStats.Add(stat);
        await dbContext.SaveChangesAsync(cancellationToken);

        await realtimeNotifier.NotifyAsync($"match-stats:{request.MatchResultId}", cancellationToken);
        await realtimeNotifier.NotifyAsync("stats", cancellationToken);

        return new PlayerMatchStatDto(
            stat.Id, player.Id, player.DisplayName, stat.Kills, stat.Deaths, stat.Assists, stat.Adr, stat.HeadshotPercentage, stat.Rating);
    }

    #endregion
}
