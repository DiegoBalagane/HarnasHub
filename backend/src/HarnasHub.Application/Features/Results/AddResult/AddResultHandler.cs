using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Results.Shared;
using HarnasHub.Core.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Results.AddResult;

/// <summary>Handles <see cref="AddResultCommand"/> by persisting the new match result.</summary>
public class AddResultHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<AddResultCommand, ErrorOr<MatchResultDto>>
{
	#region Public Methods

	public async Task<ErrorOr<MatchResultDto>> Handle(AddResultCommand request, CancellationToken cancellationToken)
	{
		var result = new MatchResult
		{
			Id = Guid.NewGuid(),
			Opponent = request.Opponent,
			OurScore = request.OurScore,
			OpponentScore = request.OpponentScore,
			MapName = request.MapName,
			DemoUrl = request.DemoUrl,
			Notes = request.Notes,
			PlayedAtUtc = request.PlayedAtUtc,
			Category = request.Category,
			TournamentId = request.TournamentId,
			LeagueId = request.LeagueId,
			CreatedByUserId = currentUser.UserId,
			CreatedAtUtc = DateTime.UtcNow
		};

		dbContext.MatchResults.Add(result);
		await dbContext.SaveChangesAsync(cancellationToken);

		await realtimeNotifier.NotifyAsync("results", cancellationToken);
		await realtimeNotifier.NotifyAsync("stats", cancellationToken);
		await realtimeNotifier.NotifyAsync("dashboard", cancellationToken);

		var tournament = request.TournamentId is null
			? null
			: await dbContext.Tournaments.FirstOrDefaultAsync(t => t.Id == request.TournamentId, cancellationToken);
		var league = request.LeagueId is null
			? null
			: await dbContext.Leagues.FirstOrDefaultAsync(l => l.Id == request.LeagueId, cancellationToken);

		return new MatchResultDto(
			result.Id,
			result.Opponent,
			result.OurScore,
			result.OpponentScore,
			result.MapName,
			result.DemoUrl,
			result.Notes,
			result.PlayedAtUtc,
			result.Category,
			tournament?.Id,
			tournament?.Name,
			league?.Id,
			league?.Name,
			league?.Season,
			league?.Type);
	}

	#endregion
}
