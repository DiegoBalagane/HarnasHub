using ErrorOr;
using HarnasHub.Application.Features.Roster.Shared;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.Roster.UpdateAccessLevel;

/// <summary>Changes a team member's access level. Manager only — enforced at the endpoint.</summary>
public record UpdateAccessLevelCommand(Guid UserId, AccessLevel AccessLevel) : IRequest<ErrorOr<TeamMemberDto>>;
