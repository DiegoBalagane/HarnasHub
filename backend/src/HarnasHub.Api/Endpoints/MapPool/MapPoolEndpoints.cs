using HarnasHub.Api.Common;
using HarnasHub.Application.Features.MapPool.GetMapPool;
using HarnasHub.Application.Features.MapPool.SetMapPoolEntry;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Api.Endpoints.MapPool;

/// <summary>Map pool endpoints under /api/map-pool: per-map status plus the team's record on each map.</summary>
public class MapPoolEndpoints : IEndpoint
{
	#region Public Methods

	public static void MapEndpoints(IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/map-pool").WithTags("MapPool").RequireAuthorization(AuthorizationPolicies.TeamMember);

		group.MapGet("/", async (MatchCategory? category, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetMapPoolQuery(category), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapPut("/{mapName}", async (
			MapName mapName,
			SetMapPoolEntryRequest request,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new SetMapPoolEntryCommand(mapName, request.Status, request.Note), cancellationToken);
			return result.Match(success => Results.NoContent(), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));
	}

	#endregion
}

/// <summary>Request body for PUT /api/map-pool/{mapName}; a null <paramref name="Status"/> clears the map's classification.</summary>
public record SetMapPoolEntryRequest(MapPoolStatus? Status, string? Note);
