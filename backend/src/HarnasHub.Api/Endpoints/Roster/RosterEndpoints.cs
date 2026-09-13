using HarnasHub.Api.Common;
using HarnasHub.Application.Features.Roster.GetRoster;
using HarnasHub.Application.Features.Roster.UpdateUserRole;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Api.Endpoints.Roster;

/// <summary>Team roster endpoints under /api/roster.</summary>
public class RosterEndpoints : IEndpoint
{
    #region Public Methods

    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/roster").WithTags("Roster").RequireAuthorization();

        group.MapGet("/", async (ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetRosterQuery(), cancellationToken);

            return result.Match(
                success => Results.Ok(success),
                errors => errors.ToProblemResult());
        });

        group.MapPatch("/{userId:guid}/role", async (
            Guid userId,
            UpdateUserRoleRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new UpdateUserRoleCommand(userId, request.Role), cancellationToken);

            return result.Match(
                success => Results.Ok(success),
                errors => errors.ToProblemResult());
        }).RequireAuthorization(policy => policy.RequireRole("Manager"));
    }

    #endregion
}

/// <summary>Request body for PATCH /api/roster/{userId}/role.</summary>
public record UpdateUserRoleRequest(UserRole Role);
