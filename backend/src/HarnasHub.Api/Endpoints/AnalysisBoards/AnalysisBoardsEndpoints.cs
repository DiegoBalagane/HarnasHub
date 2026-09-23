using HarnasHub.Api.Common;
using HarnasHub.Application.Features.AnalysisBoards.CreateBoard;
using HarnasHub.Application.Features.AnalysisBoards.DeleteBoard;
using HarnasHub.Application.Features.AnalysisBoards.GetBoards;
using HarnasHub.Application.Features.AnalysisBoards.PresignBoardImageUpload;
using HarnasHub.Application.Features.AnalysisBoards.UpdateBoard;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Api.Endpoints.AnalysisBoards;

/// <summary>Freehand analysis-drawing endpoints under /api/analysis-boards.</summary>
public class AnalysisBoardsEndpoints : IEndpoint
{
	#region Public Methods

	public static void MapEndpoints(IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/analysis-boards").WithTags("AnalysisBoards").RequireAuthorization(AuthorizationPolicies.TeamMember);

		group.MapGet("/", async (MapName? mapName, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetBoardsQuery(mapName), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapPost("/", async (CreateBoardRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new CreateBoardCommand(request.MapName, request.Title, request.BackgroundImageObjectKey, request.StrokesJson);
			var result = await sender.Send(command, cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		group.MapPatch("/{boardId:guid}", async (Guid boardId, UpdateBoardRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new UpdateBoardCommand(boardId, request.Title, request.BackgroundImageObjectKey, request.StrokesJson);
			var result = await sender.Send(command, cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		group.MapDelete("/{boardId:guid}", async (Guid boardId, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new DeleteBoardCommand(boardId), cancellationToken);
			return result.Match(success => Results.NoContent(), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		group.MapPost("/presign-image-upload", async (ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new PresignBoardImageUploadCommand(), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));
	}

	#endregion
}

/// <summary>Request body for POST /api/analysis-boards.</summary>
public record CreateBoardRequest(MapName MapName, string Title, string? BackgroundImageObjectKey, string StrokesJson);

/// <summary>Request body for PATCH /api/analysis-boards/{boardId}.</summary>
public record UpdateBoardRequest(string Title, string? BackgroundImageObjectKey, string StrokesJson);
