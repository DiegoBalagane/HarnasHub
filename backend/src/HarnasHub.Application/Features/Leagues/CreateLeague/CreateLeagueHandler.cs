using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Leagues.Shared;
using HarnasHub.Core.Entities;
using MediatR;

namespace HarnasHub.Application.Features.Leagues.CreateLeague;

/// <summary>Handles <see cref="CreateLeagueCommand"/> by persisting the new league season.</summary>
public class CreateLeagueHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<CreateLeagueCommand, ErrorOr<LeagueDto>>
{
	#region Public Methods

	public async Task<ErrorOr<LeagueDto>> Handle(CreateLeagueCommand request, CancellationToken cancellationToken)
	{
		var league = new League
		{
			Id = Guid.NewGuid(),
			Name = request.Name,
			Season = request.Season,
			Type = request.Type,
			CreatedByUserId = currentUser.UserId,
			CreatedAtUtc = DateTime.UtcNow
		};

		dbContext.Leagues.Add(league);
		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("leagues", cancellationToken);

		return new LeagueDto(league.Id, league.Name, league.Season, league.Type);
	}

	#endregion
}
