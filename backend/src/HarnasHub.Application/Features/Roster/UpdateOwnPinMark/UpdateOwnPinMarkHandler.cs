using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Roster.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Roster.UpdateOwnPinMark;

/// <summary>Handles <see cref="UpdateOwnPinMarkCommand"/> by storing the caller's own pin mark — open to every roster member, unlike the pin colour.</summary>
public class UpdateOwnPinMarkHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<UpdateOwnPinMarkCommand, ErrorOr<TeamMemberDto>>
{
	#region Public Methods

	public async Task<ErrorOr<TeamMemberDto>> Handle(UpdateOwnPinMarkCommand request, CancellationToken cancellationToken)
	{
		var userId = currentUser.UserId;
		var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

		if (user is null)
		{
			return RosterErrors.UserNotFound;
		}

		user.PinMark = string.IsNullOrEmpty(request.NewPinMark) ? null : request.NewPinMark;
		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("roster", cancellationToken);
		await realtimeNotifier.NotifyAsync("map-strategy", cancellationToken);

		return user.ToTeamMemberDto();
	}

	#endregion
}
