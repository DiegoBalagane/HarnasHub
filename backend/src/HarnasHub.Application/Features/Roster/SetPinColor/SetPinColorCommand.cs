using ErrorOr;
using HarnasHub.Application.Features.Roster.Shared;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.Roster.SetPinColor;

/// <summary>Sets (or clears, when null) a team member's map-radar pin colour. Manager only — enforced at the endpoint. Rejected unless the target is on the Main roster.</summary>
public record SetPinColorCommand(Guid UserId, PinColor? NewPinColor) : IRequest<ErrorOr<TeamMemberDto>>;
