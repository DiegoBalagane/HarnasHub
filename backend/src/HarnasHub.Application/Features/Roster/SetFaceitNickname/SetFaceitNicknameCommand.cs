using ErrorOr;
using HarnasHub.Application.Features.Roster.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Roster.SetFaceitNickname;

/// <summary>Sets (or, with a null value, clears) a team member's manual FACEIT nickname. Manager only — enforced at the endpoint.
/// Used as a fallback when the player's SteamID64 is missing or not linked to a FACEIT account.</summary>
public record SetFaceitNicknameCommand(Guid UserId, string? Nickname) : IRequest<ErrorOr<TeamMemberDto>>;
