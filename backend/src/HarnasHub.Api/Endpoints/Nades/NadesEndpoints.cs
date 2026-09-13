using HarnasHub.Api.Common;
using HarnasHub.Application.Features.Nades.AddNade;
using HarnasHub.Application.Features.Nades.DeleteNade;
using HarnasHub.Application.Features.Nades.GetNades;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Api.Endpoints.Nades;

/// <summary>Per-map nade lineup library endpoints under /api/nades.</summary>
public class NadesEndpoints : IEndpoint
{
    #region Public Methods

    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/nades").WithTags("Nades").RequireAuthorization();

        group.MapGet("/", async (
            string? mapName,
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
    }

    #endregion
}

/// <summary>Request body for POST /api/nades.</summary>
public record AddNadeRequest(string MapName, GrenadeType Type, string Title, string? Description, string? YoutubeUrl);
