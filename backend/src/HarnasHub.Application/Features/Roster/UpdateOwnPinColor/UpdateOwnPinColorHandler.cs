using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Roster.Shared;
using HarnasHub.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Roster.UpdateOwnPinColor;

/// <summary>Handles <see cref="UpdateOwnPinColorCommand"/> by storing the caller's own pin colour — rejected unless they're on the Main roster.</summary>
public class UpdateOwnPinColorHandler(
	IApplicationDbContext dbContext,
	ICurrentUserService currentUser,
	IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<UpdateOwnPinColorCommand, ErrorOr<TeamMemberDto>>
{
	#region Public Methods

	public async Task<ErrorOr<TeamMemberDto>> Handle(
		UpdateOwnPinColorCommand request,
		CancellationToken cancellationToken)
	{
		var userId = currentUser.UserId;
		var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

		if (user is null)
		{
			return RosterErrors.UserNotFound;
		}

		if (request.NewPinColor.HasValue && user.RosterSlot != RosterSlot.Main)
		{
			return RosterErrors.PinColorRequiresMainRoster;
		}

		user.PinColor = request.NewPinColor;
		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("roster", cancellationToken);
		await realtimeNotifier.NotifyAsync("map-strategy", cancellationToken);

		return user.ToTeamMemberDto();
	}

	#endregion
}
