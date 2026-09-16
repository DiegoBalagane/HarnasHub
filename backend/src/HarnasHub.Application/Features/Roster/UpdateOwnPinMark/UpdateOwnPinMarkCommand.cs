using ErrorOr;
using HarnasHub.Application.Features.Roster.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Roster.UpdateOwnPinMark;

/// <summary>Sets (or clears, when null) the caller's map-radar pin mark — a single character shown on their pin instead of their initials; the target is always the current user.</summary>
public record UpdateOwnPinMarkCommand(string? NewPinMark) : IRequest<ErrorOr<TeamMemberDto>>;
