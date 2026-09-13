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
        return await dbContext.Users
            .OrderBy(u => u.DisplayName)
            .Select(u => new TeamMemberDto(u.Id, u.DisplayName, u.Role.ToString()))
            .ToListAsync(cancellationToken);
    }

    #endregion
}
