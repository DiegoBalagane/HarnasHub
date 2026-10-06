using ErrorOr;
using HarnasHub.Application.Features.Roster.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Roster.SetVisibility;

/// <summary>Sets whether a team member appears in team stats and in the availability calendar. Manager only — enforced at the
/// endpoint. Hiding never deletes data and can be undone at any time.</summary>
public record SetVisibilityCommand(Guid UserId, bool ShowInStats, bool ShowInCalendar) : IRequest<ErrorOr<TeamMemberDto>>;
