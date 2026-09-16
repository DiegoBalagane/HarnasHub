using ErrorOr;
using HarnasHub.Application.Features.Roster.Shared;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.Roster.UpdateOwnPinColor;

/// <summary>Sets (or clears, when null) the caller's map-radar pin colour; the target is always the current user. Only allowed for Main-roster players.</summary>
public record UpdateOwnPinColorCommand(PinColor? NewPinColor) : IRequest<ErrorOr<TeamMemberDto>>;
