using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Results.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Results.UpdateResult;

/// <summary>Handles <see cref="UpdateResultCommand"/>.</summary>
public class UpdateResultHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<UpdateResultCommand, ErrorOr<MatchResultDto>>
{
	#region Public Methods

	public async Task<ErrorOr<MatchResultDto>> Handle(UpdateResultCommand request, CancellationToken cancellationToken)
	{
		var result = await dbContext.MatchResults.FirstOrDefaultAsync(r => r.Id == request.MatchResultId, cancellationToken);

		if (result is null)
		{
			return ResultErrors.MatchNotFound;
		}

		result.Opponent = request.Opponent;
		result.OurScore = request.OurScore;
		result.OpponentScore = request.OpponentScore;
		result.MapName = request.MapName;
		result.DemoUrl = request.DemoUrl;
		result.Notes = request.Notes;
		result.PlayedAtUtc = request.PlayedAtUtc;
		result.Category = request.Category;
		result.TournamentId = request.TournamentId;
		result.LeagueId = request.LeagueId;

		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("results", cancellationToken);
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
