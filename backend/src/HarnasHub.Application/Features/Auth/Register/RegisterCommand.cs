using ErrorOr;
using HarnasHub.Application.Features.Auth.Shared;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.Auth.Register;

/// <summary>Creates a new team member account.</summary>
public record RegisterCommand(
    string Email,
    string DisplayName,
    string Password,
    UserRole Role) : IRequest<ErrorOr<AuthResultDto>>;
