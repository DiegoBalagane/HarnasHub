using ErrorOr;
using HarnasHub.Application.Features.Auth.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Auth.RefreshSession;

/// <summary>Re-issues the caller's JWT from their current DB state, so a role change (e.g. Guest promoted to Player) applies without a fresh Discord login.</summary>
public record RefreshSessionCommand : IRequest<ErrorOr<AuthResultDto>>;
