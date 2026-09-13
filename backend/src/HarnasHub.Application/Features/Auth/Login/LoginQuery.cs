using ErrorOr;
using HarnasHub.Application.Features.Auth.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Auth.Login;

/// <summary>Authenticates a user by email and password.</summary>
public record LoginQuery(string Email, string Password) : IRequest<ErrorOr<AuthResultDto>>;
