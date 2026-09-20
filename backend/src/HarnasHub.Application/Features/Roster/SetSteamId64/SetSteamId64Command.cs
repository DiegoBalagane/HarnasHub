using ErrorOr;
using HarnasHub.Application.Features.Roster.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Roster.SetSteamId64;

/// <summary>Sets (or, with a null/blank value, clears) a team member's SteamID64. Manager only — enforced at the
/// endpoint — for cases where a player hasn't set their own yet (see <c>UpdateOwnSteamId64Command</c> for the self-service version).</summary>
public record SetSteamId64Command(Guid UserId, string? SteamId64) : IRequest<ErrorOr<TeamMemberDto>>;
