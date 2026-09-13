using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Roster.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Roster.UpdateUserRole;

/// <summary>Handles <see cref="UpdateUserRoleCommand"/> by updating the target user's role.</summary>
public class UpdateUserRoleHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IRealtimeNotifier realtimeNotifier)
    : IRequestHandler<UpdateUserRoleCommand, ErrorOr<TeamMemberDto>>
{
    #region Public Methods

    public async Task<ErrorOr<TeamMemberDto>> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
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

        user.Role = request.Role;
        await dbContext.SaveChangesAsync(cancellationToken);
        await realtimeNotifier.NotifyAsync("roster", cancellationToken);

        return new TeamMemberDto(user.Id, user.DisplayName, user.Role.ToString());
    }

    #endregion
}
