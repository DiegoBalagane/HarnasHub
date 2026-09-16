using ErrorOr;
using HarnasHub.Application.Features.Roster.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Roster.UpdateOwnNickname;

/// <summary>Sets the in-game nickname of the caller; the target is always the current user, never a route parameter.</summary>
public record UpdateOwnNicknameCommand(string Nickname) : IRequest<ErrorOr<TeamMemberDto>>;
