using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Tournaments.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Tournaments.DeleteTournament;

/// <summary>Handles <see cref="DeleteTournamentCommand"/>.</summary>
public class DeleteTournamentHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<DeleteTournamentCommand, ErrorOr<Success>>
{
	#region Public Methods

	public async Task<ErrorOr<Success>> Handle(DeleteTournamentCommand request, CancellationToken cancellationToken)
	{
		var tournament = await dbContext.Tournaments.FirstOrDefaultAsync(t => t.Id == request.TournamentId, cancellationToken);

		if (tournament is null)
		{
			return TournamentErrors.TournamentNotFound;
		}

		dbContext.Tournaments.Remove(tournament);
		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("tournaments", cancellationToken);
		await realtimeNotifier.NotifyAsync("results", cancellationToken);

		return Result.Success;
	}

	#endregion
}
