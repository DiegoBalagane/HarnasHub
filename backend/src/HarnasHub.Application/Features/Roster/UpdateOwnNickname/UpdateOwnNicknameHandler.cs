using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Roster.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Roster.UpdateOwnNickname;

/// <summary>Handles <see cref="UpdateOwnNicknameCommand"/> by storing the caller's own in-game nickname.</summary>
public class UpdateOwnNicknameHandler(
	IApplicationDbContext dbContext,
	ICurrentUserService currentUser,
	IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<UpdateOwnNicknameCommand, ErrorOr<TeamMemberDto>>
{
	#region Public Methods

	public async Task<ErrorOr<TeamMemberDto>> Handle(
		UpdateOwnNicknameCommand request,
		CancellationToken cancellationToken)
	{
		var userId = currentUser.UserId;
		var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

		if (user is null)
		{
			return RosterErrors.UserNotFound;
		}

		user.InGameNickname = string.IsNullOrWhiteSpace(request.Nickname) ? null : request.Nickname.Trim();
		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("roster", cancellationToken);

		return user.ToTeamMemberDto();
	}

	#endregion
}
