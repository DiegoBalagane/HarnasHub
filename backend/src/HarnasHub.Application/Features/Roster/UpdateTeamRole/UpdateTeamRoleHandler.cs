using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Roster.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Roster.UpdateTeamRole;

/// <summary>Handles <see cref="UpdateTeamRoleCommand"/> by assigning the target user's in-game role.</summary>
public class UpdateTeamRoleHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<UpdateTeamRoleCommand, ErrorOr<TeamMemberDto>>
{
	#region Public Methods

	public async Task<ErrorOr<TeamMemberDto>> Handle(UpdateTeamRoleCommand request, CancellationToken cancellationToken)
	{
		var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

		if (user is null)
		{
			return RosterErrors.UserNotFound;
		}

		user.TeamRole = request.NewTeamRole;
		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("roster", cancellationToken);

		return user.ToTeamMemberDto();
	}

	#endregion
}
