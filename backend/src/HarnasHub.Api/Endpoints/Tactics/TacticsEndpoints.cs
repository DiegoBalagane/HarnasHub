using HarnasHub.Api.Common;
using HarnasHub.Application.Features.Tactics.CreateTactic;
using HarnasHub.Application.Features.Tactics.DeleteTactic;
using HarnasHub.Application.Features.Tactics.GetTacticDetail;
using HarnasHub.Application.Features.Tactics.GetTactics;
using HarnasHub.Application.Features.Tactics.Shared;
using HarnasHub.Application.Features.Tactics.UpdateTactic;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Api.Endpoints.Tactics;

/// <summary>Saved team-tactic library endpoints under /api/tactics.</summary>
public class TacticsEndpoints : IEndpoint
{
	#region Public Methods

	public static void MapEndpoints(IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/tactics").WithTags("Tactics").RequireAuthorization(AuthorizationPolicies.TeamMember);
		var coachOrManager = "Coach,Manager".Split(',');

		group.MapGet("/", async (
			MapName? mapName,
			MapSide? side,
			EconomyType? economy,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetTacticsQuery(mapName, side, economy), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapGet("/{tacticId:guid}", async (Guid tacticId, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetTacticDetailQuery(tacticId), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapPost("/", async (CreateTacticRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new CreateTacticCommand(request.MapName, request.Side, request.Name, request.Economy, request.Note);
			var result = await sender.Send(command, cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole(coachOrManager));

		group.MapPut("/{tacticId:guid}", async (
			Guid tacticId,
			UpdateTacticRequest request,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var command = new UpdateTacticCommand(tacticId, request.Name, request.Economy, request.Note, request.Points);
			var result = await sender.Send(command, cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole(coachOrManager));

		group.MapDelete("/{tacticId:guid}", async (Guid tacticId, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new DeleteTacticCommand(tacticId), cancellationToken);
			return result.Match(success => Results.NoContent(), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole(coachOrManager));
	}

	#endregion
}

/// <summary>Request body for POST /api/tactics.</summary>
public record CreateTacticRequest(MapName MapName, MapSide Side, string Name, EconomyType Economy, string? Note);

/// <summary>Request body for PUT /api/tactics/{tacticId}.</summary>
public record UpdateTacticRequest(string Name, EconomyType Economy, string? Note, List<TacticPointInput> Points);
