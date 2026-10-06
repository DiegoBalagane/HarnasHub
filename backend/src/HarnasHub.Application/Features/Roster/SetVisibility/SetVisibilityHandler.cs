using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Roster.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Roster.SetVisibility;

/// <summary>Handles <see cref="SetVisibilityCommand"/> by storing the target user's stats/calendar visibility flags.</summary>
public class SetVisibilityHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<SetVisibilityCommand, ErrorOr<TeamMemberDto>>
{
	#region Public Methods

	public async Task<ErrorOr<TeamMemberDto>> Handle(SetVisibilityCommand request, CancellationToken cancellationToken)
	{
		var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

		if (user is null)
		{
			return RosterErrors.UserNotFound;
		}

		user.ShowInStats = request.ShowInStats;
		user.ShowInCalendar = request.ShowInCalendar;
		await dbContext.SaveChangesAsync(cancellationToken);

		foreach (var topic in new[] { "roster", "availability-week", "dashboard", "stats" })
		{
			await realtimeNotifier.NotifyAsync(topic, cancellationToken);
		}

		return user.ToTeamMemberDto();
	}

	#endregion
}
