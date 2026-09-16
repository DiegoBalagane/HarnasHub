using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Results.Shared;
using HarnasHub.Core.Entities;
using MediatR;

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
			CreatedByUserId = currentUser.UserId,
			CreatedAtUtc = DateTime.UtcNow
		};

		dbContext.MatchResults.Add(result);
		await dbContext.SaveChangesAsync(cancellationToken);

		await realtimeNotifier.NotifyAsync("results", cancellationToken);
		await realtimeNotifier.NotifyAsync("stats", cancellationToken);
		await realtimeNotifier.NotifyAsync("dashboard", cancellationToken);

		return new MatchResultDto(
			result.Id,
			result.Opponent,
			result.OurScore,
			result.OpponentScore,
			result.MapName,
			result.DemoUrl,
			result.Notes,
			result.PlayedAtUtc);
	}

	#endregion
}
