using ErrorOr;
using HarnasHub.Application.Features.Roster.Shared;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.Roster.UpdateTeamRole;

/// <summary>Sets (or clears, when null) a team member's in-game role. Coach/Manager only — enforced at the endpoint.</summary>
public record UpdateTeamRoleCommand(Guid UserId, TeamRole? NewTeamRole) : IRequest<ErrorOr<TeamMemberDto>>;
