using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Roster.Shared;
using HarnasHub.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Roster.SetPinColor;

/// <summary>Handles <see cref="SetPinColorCommand"/> by storing the target user's pin colour — rejected unless they're on the Main roster.</summary>
public class SetPinColorHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<SetPinColorCommand, ErrorOr<TeamMemberDto>>
{
	#region Public Methods

	public async Task<ErrorOr<TeamMemberDto>> Handle(SetPinColorCommand request, CancellationToken cancellationToken)
	{
		var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

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
