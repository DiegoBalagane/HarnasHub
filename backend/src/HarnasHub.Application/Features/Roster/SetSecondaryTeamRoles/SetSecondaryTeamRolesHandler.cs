using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Roster.Shared;
using HarnasHub.Core.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Roster.SetSecondaryTeamRoles;

/// <summary>Handles <see cref="SetSecondaryTeamRolesCommand"/> by replacing the target user's full set of secondary roles.</summary>
public class SetSecondaryTeamRolesHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<SetSecondaryTeamRolesCommand, ErrorOr<TeamMemberDto>>
{
	#region Public Methods

	public async Task<ErrorOr<TeamMemberDto>> Handle(
		SetSecondaryTeamRolesCommand request,
		CancellationToken cancellationToken)
	{
		var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

		if (user is null)
		{
			return RosterErrors.UserNotFound;
		}

		if (user.TeamRole.HasValue && request.TeamRoles.Contains(user.TeamRole.Value))
		{
			return RosterErrors.SecondaryRoleMatchesPrimary;
		}

		var existing = await dbContext.UserSecondaryTeamRoles
			.Where(r => r.UserId == request.UserId)
			.ToListAsync(cancellationToken);

		dbContext.UserSecondaryTeamRoles.RemoveRange(existing);

		foreach (var teamRole in request.TeamRoles)
		{
			dbContext.UserSecondaryTeamRoles.Add(new UserSecondaryTeamRole
			{
				Id = Guid.NewGuid(),
				UserId = request.UserId,
				TeamRole = teamRole
			});
		}

		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("roster", cancellationToken);

		return user.ToTeamMemberDto(request.TeamRoles.Select(r => r.ToString()).ToList());
	}

	#endregion
}
