using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Tournaments.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Tournaments.GetTournaments;

/// <summary>Handles <see cref="GetTournamentsQuery"/>.</summary>
public class GetTournamentsHandler(IApplicationDbContext dbContext)
	: IRequestHandler<GetTournamentsQuery, ErrorOr<List<TournamentDto>>>
{
	#region Public Methods

	public async Task<ErrorOr<List<TournamentDto>>> Handle(GetTournamentsQuery request, CancellationToken cancellationToken)
	{
		return await dbContext.Tournaments
			.OrderBy(t => t.Name)
			.Select(t => new TournamentDto(t.Id, t.Name))
			.ToListAsync(cancellationToken);
	}

	#endregion
}
