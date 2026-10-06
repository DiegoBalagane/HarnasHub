using HarnasHub.Api.Common;
using HarnasHub.Application.Features.OpponentManagement.DeleteOpponent;
using HarnasHub.Application.Features.OpponentManagement.GetOpponentDeletePreview;
using HarnasHub.Application.Features.OpponentManagement.HideOpponent;
using HarnasHub.Application.Features.OpponentManagement.RenameOpponent;
using HarnasHub.Application.Features.OpponentManagement.UnhideOpponent;
using MediatR;

namespace HarnasHub.Api.Endpoints.Opponents;

/// <summary>Delete, hide/unhide and rename/merge endpoints for opponents under /api/opponents (Coach/Manager only).</summary>
public class OpponentManagementEndpoints : IEndpoint
{
	#region Public Methods

	public static void MapEndpoints(IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/opponents").WithTags("OpponentManagement")
			.RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		// Name in the query string, not the path: team names can contain "/" or "?" which would break a route segment.
		group.MapGet("/delete-preview", async (string name, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetOpponentDeletePreviewQuery(name), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapDelete("/", async (string name, bool? includeHistory, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new DeleteOpponentCommand(name, includeHistory ?? false), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapPost("/hide", async (OpponentNameRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new HideOpponentCommand(request.Name), cancellationToken);
			return result.Match(success => Results.NoContent(), errors => errors.ToProblemResult());
		});

		group.MapPost("/unhide", async (OpponentNameRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new UnhideOpponentCommand(request.Name), cancellationToken);
			return result.Match(success => Results.NoContent(), errors => errors.ToProblemResult());
		});

		group.MapPost("/rename", async (RenameOpponentRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new RenameOpponentCommand(request.From, request.To), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});
	}

	#endregion
}

/// <summary>Request body for POST /api/opponents/hide and /unhide.</summary>
public record OpponentNameRequest(string Name);

/// <summary>Request body for POST /api/opponents/rename.</summary>
public record RenameOpponentRequest(string From, string To);
