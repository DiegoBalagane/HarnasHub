using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Roster.Shared;
using HarnasHub.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Roster.SetIsCoach;

/// <summary>Handles <see cref="SetIsCoachCommand"/> by toggling the target user's coach tag, which grants coach privileges on top of their access level.</summary>
public class SetIsCoachHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<SetIsCoachCommand, ErrorOr<TeamMemberDto>>
{
	#region Public Methods

	public async Task<ErrorOr<TeamMemberDto>> Handle(SetIsCoachCommand request, CancellationToken cancellationToken)
	{
		var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

		if (user is null)
		{
			return RosterErrors.UserNotFound;
		}

		// A Guest sees no team data at all; tagging one as Coach would let them in through the TeamMember policy
		// (it OR-matches any role claim) without a Manager ever deliberately granting access.
		if (request.IsCoach && user.AccessLevel == AccessLevel.Guest)
		{
			return RosterErrors.GuestCannotBeCoach;
		}

		// Self-edit is deliberately allowed: unlike the access level, the coach tag never changes the caller's own
		// permission level in a way that could lock the last Manager out of managing the team.
		user.IsCoach = request.IsCoach;
		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("roster", cancellationToken);

		// The coach tag decides who shows up in the availability calendar, so the weekly grid has to refresh too.
		await realtimeNotifier.NotifyAsync("availability-week", cancellationToken);

		return user.ToTeamMemberDto();
	}

	#endregion
}
