using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Roster.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Roster.UpdateOwnSteamId64;

/// <summary>Handles <see cref="UpdateOwnSteamId64Command"/> by storing the caller's own SteamID64.</summary>
public class UpdateOwnSteamId64Handler(
	IApplicationDbContext dbContext,
	ICurrentUserService currentUser,
	IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<UpdateOwnSteamId64Command, ErrorOr<TeamMemberDto>>
{
	#region Public Methods

	public async Task<ErrorOr<TeamMemberDto>> Handle(
		UpdateOwnSteamId64Command request,
		CancellationToken cancellationToken)
	{
		var userId = currentUser.UserId;
		var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

		if (user is null)
		{
			return RosterErrors.UserNotFound;
		}

		user.SteamId64 = string.IsNullOrWhiteSpace(request.SteamId64) ? null : request.SteamId64.Trim();
		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("roster", cancellationToken);

		return user.ToTeamMemberDto();
	}

	#endregion
}
