using HarnasHub.Api.Common;
using HarnasHub.Application.Features.Nades.AddNade;
using HarnasHub.Application.Features.Nades.DeleteNade;
using HarnasHub.Application.Features.Nades.GetNades;
using HarnasHub.Application.Features.Nades.UpdateNadePosition;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Api.Endpoints.Nades;

/// <summary>Per-map nade lineup library endpoints under /api/nades.</summary>
public class NadesEndpoints : IEndpoint
{
	#region Public Methods

	public static void MapEndpoints(IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/nades").WithTags("Nades").RequireAuthorization(AuthorizationPolicies.TeamMember);

		group.MapGet("/", async (
			MapName? mapName,
			GrenadeType? type,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetNadesQuery(mapName, type), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapPost("/", async (AddNadeRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new AddNadeCommand(request.MapName, request.Type, request.Title, request.Description, request.YoutubeUrl);
			var result = await sender.Send(command, cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapDelete("/{nadeId:guid}", async (Guid nadeId, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new DeleteNadeCommand(nadeId), cancellationToken);
			return result.Match(success => Results.NoContent(), errors => errors.ToProblemResult());
		});

		group.MapPatch("/{nadeId:guid}/position", async (
			Guid nadeId,
			UpdateNadePositionRequest request,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new UpdateNadePositionCommand(nadeId, request.X, request.Y), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});
	}

	#endregion
}

/// <summary>Request body for POST /api/nades.</summary>
public record AddNadeRequest(MapName MapName, GrenadeType Type, string Title, string? Description, string? YoutubeUrl);

/// <summary>Request body for PATCH /api/nades/{nadeId}/position. Both fields null clears the pin.</summary>
public record UpdateNadePositionRequest(float? X, float? Y);
