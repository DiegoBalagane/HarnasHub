using ErrorOr;
using HarnasHub.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Stats.GetMyStatsHistory;

/// <summary>Handles <see cref="GetMyStatsHistoryQuery"/>.</summary>
public class GetMyStatsHistoryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser)
    : IRequestHandler<GetMyStatsHistoryQuery, ErrorOr<List<PlayerStatHistoryEntryDto>>>
{
    #region Public Methods

    public async Task<ErrorOr<List<PlayerStatHistoryEntryDto>>> Handle(
        GetMyStatsHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;

        return await (
            from stat in dbContext.PlayerMatchStats
            join match in dbContext.MatchResults on stat.MatchResultId equals match.Id
            where stat.UserId == userId
            orderby match.PlayedAtUtc
            select new PlayerStatHistoryEntryDto(
                match.Id, match.PlayedAtUtc, match.Opponent, stat.Kills, stat.Deaths, stat.Assists, stat.Adr, stat.HeadshotPercentage, stat.Rating))
            .ToListAsync(cancellationToken);
    }

    #endregion
}
