using HarnasHub.Api.Common;
using HarnasHub.Application.Features.GamePlan.GetEventGamePlan;
using HarnasHub.Application.Features.GamePlan.SetEventGamePlan;
using MediatR;

namespace HarnasHub.Api.Endpoints.GamePlan;

/// <summary>Game plan endpoints under /api/game-plans/{eventId}: the coach's notes plus tactics and boards attached to an event.</summary>
public class GamePlanEndpoints : IEndpoint
{
	#region Public Methods

	public static void MapEndpoints(IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/game-plans").WithTags("GamePlan").RequireAuthorization(AuthorizationPolicies.TeamMember);

		group.MapGet("/{eventId:guid}", async (Guid eventId, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetEventGamePlanQuery(eventId), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapPut("/{eventId:guid}", async (
			Guid eventId,
			SetEventGamePlanRequest request,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var command = new SetEventGamePlanCommand(eventId, request.Notes, request.TacticIds, request.BoardIds);
			var result = await sender.Send(command, cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));
	}

	#endregion
}

/// <summary>Request body for PUT /api/game-plans/{eventId} — the full plan; list order is display order.</summary>
public record SetEventGamePlanRequest(string? Notes, List<Guid> TacticIds, List<Guid> BoardIds);
