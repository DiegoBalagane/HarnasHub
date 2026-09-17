using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Roster.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Roster.UpdateAccessLevel;

/// <summary>Handles <see cref="UpdateAccessLevelCommand"/> by updating the target user's access level.</summary>
public class UpdateAccessLevelHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<UpdateAccessLevelCommand, ErrorOr<TeamMemberDto>>
{
	#region Public Methods

	public async Task<ErrorOr<TeamMemberDto>> Handle(UpdateAccessLevelCommand request, CancellationToken cancellationToken)
	{
		if (request.UserId == currentUser.UserId)
		{
			return RosterErrors.CannotChangeOwnRole;
		}

		var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

		if (user is null)
		{
			return RosterErrors.UserNotFound;
		}

		user.AccessLevel = request.AccessLevel;
		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("roster", cancellationToken);

		return user.ToTeamMemberDto();
	}

	#endregion
}
