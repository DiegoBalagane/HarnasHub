using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Tournaments.Shared;
using HarnasHub.Core.Entities;
using MediatR;

namespace HarnasHub.Application.Features.Tournaments.CreateTournament;

/// <summary>Handles <see cref="CreateTournamentCommand"/> by persisting the new tournament.</summary>
public class CreateTournamentHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<CreateTournamentCommand, ErrorOr<TournamentDto>>
{
	#region Public Methods

	public async Task<ErrorOr<TournamentDto>> Handle(CreateTournamentCommand request, CancellationToken cancellationToken)
	{
		var tournament = new Tournament
		{
			Id = Guid.NewGuid(),
			Name = request.Name,
			CreatedByUserId = currentUser.UserId,
			CreatedAtUtc = DateTime.UtcNow
		};

		dbContext.Tournaments.Add(tournament);
		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("tournaments", cancellationToken);

		return new TournamentDto(tournament.Id, tournament.Name);
	}

	#endregion
}
