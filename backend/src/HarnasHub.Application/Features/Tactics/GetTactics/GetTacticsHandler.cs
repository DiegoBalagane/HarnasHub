using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Tactics.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Tactics.GetTactics;

/// <summary>Handles <see cref="GetTacticsQuery"/>.</summary>
public class GetTacticsHandler(IApplicationDbContext dbContext)
	: IRequestHandler<GetTacticsQuery, ErrorOr<List<TacticDto>>>
{
	#region Public Methods

	public async Task<ErrorOr<List<TacticDto>>> Handle(GetTacticsQuery request, CancellationToken cancellationToken)
	{
		var query = dbContext.Tactics.AsQueryable();

		if (request.MapName is not null)
		{
			query = query.Where(t => t.MapName == request.MapName);
		}

		if (request.Side is not null)
		{
			query = query.Where(t => t.Side == request.Side);
		}

		if (request.Economy is not null)
		{
			query = query.Where(t => t.Economy == request.Economy);
		}

		return await query
			.OrderBy(t => t.MapName).ThenBy(t => t.Side).ThenBy(t => t.Name)
			.Select(t => new TacticDto(
				t.Id, t.MapName, t.Side, t.Name, t.Economy.ToString(), t.Note, t.CreatedByUserId, t.Points.Count))
			.ToListAsync(cancellationToken);
	}

	#endregion
}
