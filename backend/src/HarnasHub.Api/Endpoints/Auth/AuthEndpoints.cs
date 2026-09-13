using HarnasHub.Api.Common;
using HarnasHub.Application.Features.Auth.Login;
using HarnasHub.Application.Features.Auth.Register;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Api.Endpoints.Auth;

/// <summary>Register and login endpoints under /api/auth.</summary>
public class AuthEndpoints : IEndpoint
{
    #region Public Methods

    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            // New accounts always start as Player — role upgrades are a Manager action, never chosen by the registrant.
            var command = new RegisterCommand(request.Email, request.DisplayName, request.Password, UserRole.Player);
            var result = await sender.Send(command, cancellationToken);

            return result.Match(
                success => Results.Ok(success),
                errors => errors.ToProblemResult());
        });

        group.MapPost("/login", async (LoginRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var query = new LoginQuery(request.Email, request.Password);
            var result = await sender.Send(query, cancellationToken);

            return result.Match(
                success => Results.Ok(success),
                errors => errors.ToProblemResult());
        });
    }

    #endregion
}

/// <summary>Request body for POST /api/auth/register.</summary>
public record RegisterRequest(string Email, string DisplayName, string Password);

/// <summary>Request body for POST /api/auth/login.</summary>
public record LoginRequest(string Email, string Password);
