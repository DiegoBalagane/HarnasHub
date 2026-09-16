using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Roster.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Roster.GetRoster;

/// <summary>Handles <see cref="GetRosterQuery"/> by projecting all users to <see cref="TeamMemberDto"/>.</summary>
public class GetRosterHandler(IApplicationDbContext dbContext)
	: IRequestHandler<GetRosterQuery, ErrorOr<List<TeamMemberDto>>>
{
	#region Public Methods

	public async Task<ErrorOr<List<TeamMemberDto>>> Handle(GetRosterQuery request, CancellationToken cancellationToken)
	{
		var users = await dbContext.Users
			.OrderBy(u => u.DisplayName)
			.ToListAsync(cancellationToken);

		var secondaryRoles = await dbContext.UserSecondaryTeamRoles.ToListAsync(cancellationToken);
		var secondaryRolesByUser = secondaryRoles
			.GroupBy(r => r.UserId)
			.ToDictionary(group => group.Key, group => group.Select(r => r.TeamRole.ToString()).ToList());

		return users
			.Select(u => u.ToTeamMemberDto(secondaryRolesByUser.GetValueOrDefault(u.Id)))
			.ToList();
	}

	#endregion
}
