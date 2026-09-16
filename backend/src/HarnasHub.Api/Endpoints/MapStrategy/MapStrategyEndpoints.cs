using HarnasHub.Api.Common;
using HarnasHub.Application.Features.MapStrategy.GetMapPositions;
using HarnasHub.Application.Features.MapStrategy.RemovePlayerPosition;
using HarnasHub.Application.Features.MapStrategy.SetPlayerPosition;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Api.Endpoints.MapStrategy;

/// <summary>Per-map starting-position endpoints under /api/map-strategy.</summary>
public class MapStrategyEndpoints : IEndpoint
{
	#region Public Methods

	public static void MapEndpoints(IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/map-strategy")
			.WithTags("MapStrategy")
			.RequireAuthorization(AuthorizationPolicies.TeamMember);

		group.MapGet("/{mapName}/{side}", async (
			MapName mapName,
			MapSide side,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetMapPositionsQuery(mapName, side), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapPost("/", async (
			SetPlayerPositionRequest request,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var command = new SetPlayerPositionCommand(
				request.MapName, request.Side, request.UserId, request.Label, request.X, request.Y, request.Note);
			var result = await sender.Send(command, cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		group.MapDelete("/{positionId:guid}", async (
			Guid positionId,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new RemovePlayerPositionCommand(positionId), cancellationToken);
			return result.Match(success => Results.NoContent(), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));
	}

	#endregion
}

/// <summary>Request body for POST /api/map-strategy.</summary>
public record SetPlayerPositionRequest(
	MapName MapName,
	MapSide Side,
	Guid UserId,
	string? Label,
	float X,
	float Y,
	string? Note);
