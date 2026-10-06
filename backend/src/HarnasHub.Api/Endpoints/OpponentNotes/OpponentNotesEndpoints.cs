using HarnasHub.Api.Common;
using HarnasHub.Application.Features.OpponentNotes.AddOpponentNote;
using HarnasHub.Application.Features.OpponentNotes.DeleteOpponentNote;
using HarnasHub.Application.Features.OpponentNotes.GetOpponentProfile;
using HarnasHub.Application.Features.OpponentNotes.GetOpponents;
using HarnasHub.Application.Features.OpponentNotes.UpdateOpponentNote;
using MediatR;

namespace HarnasHub.Api.Endpoints.OpponentNotes;

/// <summary>Opponent list, opponent profile and scouting note endpoints under /api/opponents.</summary>
public class OpponentNotesEndpoints : IEndpoint
{
	#region Public Methods

	public static void MapEndpoints(IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/opponents").WithTags("OpponentNotes").RequireAuthorization(AuthorizationPolicies.TeamMember);

		group.MapGet("/", async (bool? includeHidden, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetOpponentsQuery(includeHidden ?? false), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		// Name in the query string, not the path: team names can contain "/" or "?" which would break a route segment.
		group.MapGet("/profile", async (string name, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetOpponentProfileQuery(name), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapPost("/notes", async (OpponentNoteRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new AddOpponentNoteCommand(request.OpponentName, request.Content, request.MaterialUrl);
			var result = await sender.Send(command, cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		group.MapPut("/notes/{noteId:guid}", async (
			Guid noteId,
			OpponentNoteRequest request,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var command = new UpdateOpponentNoteCommand(noteId, request.OpponentName, request.Content, request.MaterialUrl);
			var result = await sender.Send(command, cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		group.MapDelete("/notes/{noteId:guid}", async (Guid noteId, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new DeleteOpponentNoteCommand(noteId), cancellationToken);
			return result.Match(success => Results.NoContent(), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));
	}

	#endregion
}

/// <summary>Request body for POST /api/opponents/notes and PUT /api/opponents/notes/{noteId}.</summary>
public record OpponentNoteRequest(string OpponentName, string Content, string? MaterialUrl);
