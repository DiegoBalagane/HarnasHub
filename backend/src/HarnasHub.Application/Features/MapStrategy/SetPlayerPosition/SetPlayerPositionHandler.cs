using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MapStrategy.Shared;
using HarnasHub.Core.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.MapStrategy.SetPlayerPosition;

/// <summary>Handles <see cref="SetPlayerPositionCommand"/> by upserting the row keyed on (map, side, user).</summary>
public class SetPlayerPositionHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<SetPlayerPositionCommand, ErrorOr<MapPositionDto>>
{
	#region Public Methods

	public async Task<ErrorOr<MapPositionDto>> Handle(SetPlayerPositionCommand request, CancellationToken cancellationToken)
	{
		var user = await dbContext.Users
			.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

		if (user is null)
		{
			return MapStrategyErrors.UserNotFound;
		}

		var position = await dbContext.MapPositionAssignments.FirstOrDefaultAsync(
			p => p.MapName == request.MapName && p.Side == request.Side && p.UserId == request.UserId,
			cancellationToken);

		if (position is null)
		{
			position = new MapPositionAssignment
			{
				Id = Guid.NewGuid(),
				MapName = request.MapName,
				Side = request.Side,
				UserId = request.UserId
			};

			dbContext.MapPositionAssignments.Add(position);
		}

		position.Label = request.Label;
		position.X = request.X;
		position.Y = request.Y;
		position.Note = request.Note;
		position.UpdatedAtUtc = DateTime.UtcNow;

		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("map-strategy", cancellationToken);

		return new MapPositionDto(
			position.Id,
			user.Id.ToString(),
			user.DisplayName,
			user.InGameNickname,
			user.TeamRole?.ToString(),
			user.PinColor?.ToString(),
			user.PinMark,
			position.Label,
			position.X,
			position.Y,
			position.Note);
	}

	#endregion
}
