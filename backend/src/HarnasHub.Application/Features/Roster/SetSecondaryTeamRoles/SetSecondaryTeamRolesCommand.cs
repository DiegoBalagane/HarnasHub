using ErrorOr;
using HarnasHub.Application.Features.Roster.Shared;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.Roster.SetSecondaryTeamRoles;

/// <summary>Replaces the full set of backup in-game roles (e.g. "second AWPer") a player covers alongside their primary <see cref="TeamRole"/>. Coach/Manager only — enforced at the endpoint.</summary>
public record SetSecondaryTeamRolesCommand(Guid UserId, List<TeamRole> TeamRoles) : IRequest<ErrorOr<TeamMemberDto>>;
