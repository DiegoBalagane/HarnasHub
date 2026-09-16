using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Roster.Shared;
using HarnasHub.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Roster.UpdateRosterSlot;

/// <summary>Handles <see cref="UpdateRosterSlotCommand"/> by assigning the target user's roster slot, keeping Main capped at five players.</summary>
public class UpdateRosterSlotHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<UpdateRosterSlotCommand, ErrorOr<TeamMemberDto>>
{
	#region Private Fields

	private const int MaxMainRosterSize = 5;

	#endregion

	#region Public Methods

	public async Task<ErrorOr<TeamMemberDto>> Handle(UpdateRosterSlotCommand request, CancellationToken cancellationToken)
	{
		var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

		if (user is null)
		{
			return RosterErrors.UserNotFound;
		}

		if (request.NewRosterSlot == RosterSlot.Main && user.RosterSlot != RosterSlot.Main)
		{
			var mainCount = await dbContext.Users.CountAsync(u => u.RosterSlot == RosterSlot.Main, cancellationToken);

			if (mainCount >= MaxMainRosterSize)
			{
				return RosterErrors.MainRosterFull;
			}
		}

		user.RosterSlot = request.NewRosterSlot;
		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("roster", cancellationToken);
		await realtimeNotifier.NotifyAsync("availability-week", cancellationToken);

		return user.ToTeamMemberDto();
	}

	#endregion
}
