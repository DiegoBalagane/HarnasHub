using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MapStrategy.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.MapStrategy.GetMapPositions;

/// <summary>Handles <see cref="GetMapPositionsQuery"/> by joining assignments with the roster in a single round trip.</summary>
public class GetMapPositionsHandler(IApplicationDbContext dbContext)
	: IRequestHandler<GetMapPositionsQuery, ErrorOr<List<MapPositionDto>>>
{
	#region Public Methods

	public async Task<ErrorOr<List<MapPositionDto>>> Handle(GetMapPositionsQuery request, CancellationToken cancellationToken)
	{
		// Guid.ToString() is done after materialization on purpose — it is not guaranteed to translate to SQL.
		var rows = await dbContext.MapPositionAssignments
			.Where(p => p.MapName == request.MapName && p.Side == request.Side)
			.Join(
				dbContext.Users,
				position => position.UserId,
				user => user.Id,
				(position, user) => new
				{
					position.Id,
					user.DisplayName,
					user.InGameNickname,
					user.TeamRole,
					user.PinColor,
					user.PinMark,
					position.UserId,
					position.Label,
					position.X,
					position.Y,
					position.Note
				})
			.OrderBy(row => row.DisplayName)
			.ToListAsync(cancellationToken);

		return rows
			.Select(row => new MapPositionDto(
				row.Id,
				row.UserId.ToString(),
				row.DisplayName,
				row.InGameNickname,
				row.TeamRole?.ToString(),
				row.PinColor?.ToString(),
				row.PinMark,
				row.Label,
				row.X,
				row.Y,
				row.Note))
			.ToList();
	}

	#endregion
}
