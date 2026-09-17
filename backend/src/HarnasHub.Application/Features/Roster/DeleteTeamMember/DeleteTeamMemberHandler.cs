using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Roster.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Roster.DeleteTeamMember;

/// <summary>
/// Handles <see cref="DeleteTeamMemberCommand"/>: permanently removes the account and everything that is only
/// his (availability, vacations, map pin, secondary roles, tasks assigned to him). Team artifacts he created —
/// match results, nade lineups, tactics, training materials, tasks he assigned to others — are left in place
/// with a dangling creator id, since none of them have a real database foreign key to <c>Users</c>.
/// </summary>
public class DeleteTeamMemberHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<DeleteTeamMemberCommand, ErrorOr<Success>>
{
	#region Public Methods

	public async Task<ErrorOr<Success>> Handle(DeleteTeamMemberCommand request, CancellationToken cancellationToken)
	{
		if (request.UserId == currentUser.UserId)
		{
			return RosterErrors.CannotDeleteOwnAccount;
		}

		var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

		if (user is null)
		{
			return RosterErrors.UserNotFound;
		}

		dbContext.Availabilities.RemoveRange(
			await dbContext.Availabilities.Where(a => a.UserId == request.UserId).ToListAsync(cancellationToken));
		dbContext.MapPositionAssignments.RemoveRange(
			await dbContext.MapPositionAssignments.Where(p => p.UserId == request.UserId).ToListAsync(cancellationToken));
		dbContext.PlayerAvailabilityDays.RemoveRange(
			await dbContext.PlayerAvailabilityDays.Where(d => d.UserId == request.UserId).ToListAsync(cancellationToken));
		dbContext.Vacations.RemoveRange(
			await dbContext.Vacations.Where(v => v.UserId == request.UserId).ToListAsync(cancellationToken));
		dbContext.UserSecondaryTeamRoles.RemoveRange(
			await dbContext.UserSecondaryTeamRoles.Where(r => r.UserId == request.UserId).ToListAsync(cancellationToken));
		dbContext.Tasks.RemoveRange(
			await dbContext.Tasks.Where(t => t.AssignedToUserId == request.UserId).ToListAsync(cancellationToken));

		dbContext.Users.Remove(user);
		await dbContext.SaveChangesAsync(cancellationToken);

		await realtimeNotifier.NotifyAsync("roster", cancellationToken);
		await realtimeNotifier.NotifyAsync("availability-week", cancellationToken);
		await realtimeNotifier.NotifyAsync("map-strategy", cancellationToken);
		await realtimeNotifier.NotifyAsync("tasks", cancellationToken);
		await realtimeNotifier.NotifyAsync("dashboard", cancellationToken);

		return Result.Success;
	}

	#endregion
}
