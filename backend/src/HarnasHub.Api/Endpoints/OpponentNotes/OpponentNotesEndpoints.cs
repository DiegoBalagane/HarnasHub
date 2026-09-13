using HarnasHub.Api.Common;
using HarnasHub.Application.Features.OpponentNotes.AddOpponentNote;
using HarnasHub.Application.Features.OpponentNotes.GetOpponentNotes;
using MediatR;

namespace HarnasHub.Api.Endpoints.OpponentNotes;

/// <summary>Opponent scouting note endpoints under /api/opponents.</summary>
public class OpponentNotesEndpoints : IEndpoint
{
    #region Public Methods

    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/opponents").WithTags("OpponentNotes").RequireAuthorization();

        group.MapGet("/", async (string? opponentName, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetOpponentNotesQuery(opponentName), cancellationToken);
            return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
        });

        group.MapPost("/", async (AddOpponentNoteRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new AddOpponentNoteCommand(request.OpponentName, request.Content, request.MaterialUrl);
            var result = await sender.Send(command, cancellationToken);
            return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
        }).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));
    }

    #endregion
}

/// <summary>Request body for POST /api/opponents.</summary>
public record AddOpponentNoteRequest(string OpponentName, string Content, string? MaterialUrl);
