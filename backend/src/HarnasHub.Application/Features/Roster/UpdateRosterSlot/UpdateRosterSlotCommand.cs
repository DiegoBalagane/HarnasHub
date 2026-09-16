using ErrorOr;
using HarnasHub.Application.Features.Roster.Shared;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.Roster.UpdateRosterSlot;

/// <summary>Sets (or clears, when null) a team member's roster slot — Main, Bench, or StandIn. Coach/Manager only — enforced at the endpoint.</summary>
public record UpdateRosterSlotCommand(Guid UserId, RosterSlot? NewRosterSlot) : IRequest<ErrorOr<TeamMemberDto>>;
