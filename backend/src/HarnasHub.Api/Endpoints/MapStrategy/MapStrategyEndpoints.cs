using HarnasHub.Api.Common;
using HarnasHub.Application.Features.MapStrategy.AddTextAnnotation;
using HarnasHub.Application.Features.MapStrategy.GetMapPositions;
using HarnasHub.Application.Features.MapStrategy.GetMapTextAnnotations;
using HarnasHub.Application.Features.MapStrategy.RemovePlayerPosition;
using HarnasHub.Application.Features.MapStrategy.RemoveTextAnnotation;
using HarnasHub.Application.Features.MapStrategy.SetPlayerPosition;
using HarnasHub.Application.Features.MapStrategy.UpdateTextAnnotation;
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

		group.MapGet("/{mapName}/{side}/text-annotations", async (
			MapName mapName,
			MapSide side,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetMapTextAnnotationsQuery(mapName, side), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapPost("/text-annotations", async (
			AddTextAnnotationRequest request,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var command = new AddTextAnnotationCommand(
				request.MapName, request.Side, request.Text, request.Color, request.FontSizePx, request.X, request.Y);
			var result = await sender.Send(command, cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		group.MapPatch("/text-annotations/{annotationId:guid}", async (
			Guid annotationId,
			UpdateTextAnnotationRequest request,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var command = new UpdateTextAnnotationCommand(
				annotationId, request.Text, request.Color, request.FontSizePx, request.X, request.Y);
			var result = await sender.Send(command, cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		group.MapDelete("/text-annotations/{annotationId:guid}", async (
			Guid annotationId,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new RemoveTextAnnotationCommand(annotationId), cancellationToken);
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

/// <summary>Request body for POST /api/map-strategy/text-annotations.</summary>
public record AddTextAnnotationRequest(
	MapName MapName,
	MapSide Side,
	string Text,
	string Color,
	int FontSizePx,
	float X,
	float Y);

/// <summary>Request body for PATCH /api/map-strategy/text-annotations/{annotationId}.</summary>
public record UpdateTextAnnotationRequest(
	string Text,
	string Color,
	int FontSizePx,
	float X,
	float Y);
