using ErrorOr;
using HarnasHub.Application.Features.Auth.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Auth.DiscordLogin;

/// <summary>Completes Discord sign-in: exchanges the OAuth code, finds-or-creates the local user, issues a JWT.</summary>
public record DiscordLoginCommand(string Code) : IRequest<ErrorOr<AuthResultDto>>;
