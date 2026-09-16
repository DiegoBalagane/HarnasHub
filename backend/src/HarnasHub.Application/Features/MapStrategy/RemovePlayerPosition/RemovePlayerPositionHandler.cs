using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MapStrategy.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.MapStrategy.RemovePlayerPosition;

/// <summary>Handles <see cref="RemovePlayerPositionCommand"/>.</summary>
public class RemovePlayerPositionHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<RemovePlayerPositionCommand, ErrorOr<Success>>
{
	#region Public Methods

	public async Task<ErrorOr<Success>> Handle(RemovePlayerPositionCommand request, CancellationToken cancellationToken)
	{
		var position = await dbContext.MapPositionAssignments
			.FirstOrDefaultAsync(p => p.Id == request.PositionId, cancellationToken);

		if (position is null)
		{
			return MapStrategyErrors.PositionNotFound;
		}

		dbContext.MapPositionAssignments.Remove(position);
		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("map-strategy", cancellationToken);

		return Result.Success;
	}

	#endregion
}
