using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Leagues.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Leagues.DeleteLeague;

/// <summary>Handles <see cref="DeleteLeagueCommand"/>.</summary>
public class DeleteLeagueHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<DeleteLeagueCommand, ErrorOr<Success>>
{
	#region Public Methods

	public async Task<ErrorOr<Success>> Handle(DeleteLeagueCommand request, CancellationToken cancellationToken)
	{
		var league = await dbContext.Leagues.FirstOrDefaultAsync(l => l.Id == request.LeagueId, cancellationToken);

		if (league is null)
		{
			return LeagueErrors.LeagueNotFound;
		}

		dbContext.Leagues.Remove(league);
		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("leagues", cancellationToken);
		await realtimeNotifier.NotifyAsync("results", cancellationToken);

		return Result.Success;
	}

	#endregion
}
