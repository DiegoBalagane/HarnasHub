using ErrorOr;
using HarnasHub.Application.Features.Roster.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Roster.UpdateOwnSteamId64;

/// <summary>Sets (or, with a null/blank value, clears) the SteamID64 of the caller, used to match demo-import stats; the target is always the current user, never a route parameter.</summary>
public record UpdateOwnSteamId64Command(string? SteamId64) : IRequest<ErrorOr<TeamMemberDto>>;
