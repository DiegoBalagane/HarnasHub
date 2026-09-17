using ErrorOr;
using HarnasHub.Application.Features.Roster.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Roster.SetIsCoach;

/// <summary>Tags (or untags) a team member as the team's coach, independent of their access level. Manager only — enforced at the endpoint.</summary>
public record SetIsCoachCommand(Guid UserId, bool IsCoach) : IRequest<ErrorOr<TeamMemberDto>>;
