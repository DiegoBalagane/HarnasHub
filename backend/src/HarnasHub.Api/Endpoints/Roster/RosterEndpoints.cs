using HarnasHub.Api.Common;
using HarnasHub.Application.Features.Roster.GetRoster;
using HarnasHub.Application.Features.Roster.UpdateOwnNickname;
using HarnasHub.Application.Features.Roster.UpdateTeamRole;
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
		var group = app.MapGroup("/api/roster").WithTags("Roster").RequireAuthorization(AuthorizationPolicies.TeamMember);

		group.MapGet("/", async (ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetRosterQuery(), cancellationToken);

			return result.Match(
				success => Results.Ok(success),
				errors => errors.ToProblemResult());
		});

		// Registered before the "/{userId:guid}/..." routes for readability; the literal "me" segment also fails
		// the :guid constraint, so ASP.NET Core routing never treats the two as ambiguous.
		group.MapPatch("/me/nickname", async (
			UpdateOwnNicknameRequest request,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new UpdateOwnNicknameCommand(request.Nickname), cancellationToken);

			return result.Match(
				success => Results.Ok(success),
				errors => errors.ToProblemResult());
		});

		group.MapPatch("/{userId:guid}/team-role", async (
			Guid userId,
			UpdateTeamRoleRequest request,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new UpdateTeamRoleCommand(userId, request.TeamRole), cancellationToken);

			return result.Match(
				success => Results.Ok(success),
				errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

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

/// <summary>Request body for PATCH /api/roster/{userId}/team-role; a null role clears the assignment.</summary>
public record UpdateTeamRoleRequest(TeamRole? TeamRole);

/// <summary>Request body for PATCH /api/roster/me/nickname.</summary>
public record UpdateOwnNicknameRequest(string Nickname);
