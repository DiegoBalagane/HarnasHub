using ErrorOr;
using HarnasHub.Application.Features.Roster.Shared;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.Roster.UpdateUserRole;

/// <summary>Changes a team member's role. Manager only — enforced at the endpoint.</summary>
public record UpdateUserRoleCommand(Guid UserId, UserRole Role) : IRequest<ErrorOr<TeamMemberDto>>;
