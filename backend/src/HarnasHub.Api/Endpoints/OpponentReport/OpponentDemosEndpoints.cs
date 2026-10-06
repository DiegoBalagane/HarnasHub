#region Usings

using HarnasHub.Api.Common;
using HarnasHub.Application.Features.Jobs.StartJob;
using HarnasHub.Application.Features.OpponentReport.AnalyzeOpponentDemo;
using HarnasHub.Application.Features.OpponentReport.DeleteOpponentDemo;
using HarnasHub.Application.Features.OpponentReport.DownloadOpponentDemos;
using HarnasHub.Application.Features.OpponentReport.GetOpponentDemos;
using HarnasHub.Application.Features.OpponentReport.GetOpponentDemoReplay;
using HarnasHub.Application.Features.OpponentReport.SetOpponentDemoTeam;
using HarnasHub.Core.Enums;
using MediatR;

#endregion

namespace HarnasHub.Api.Endpoints.OpponentReport;

/// <summary>Opponent demo endpoints (stage 5B) under /api/opponents/report/demos; tendencies themselves come with GET /api/opponents/report.</summary>
public class OpponentDemosEndpoints : IEndpoint
{
	#region Public Methods

	/// <summary>Maps list (team members), analyse / pick team / delete / FACEIT download (Coach/Manager).</summary>
	public static void MapEndpoints(IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/opponents/report/demos").WithTags("OpponentReport").RequireAuthorization(AuthorizationPolicies.TeamMember);
		string[] coachOrManager = ["Coach", "Manager"];

		group.MapGet("/", async (string name, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetOpponentDemosQuery(name), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		// The demo is uploaded via POST /api/results/analyze-demo/presign first; only its object key comes here (one call per file).
		group.MapGet("/{id:guid}/rounds/{roundNumber:int}/replay", async (Guid id, int roundNumber, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetOpponentDemoReplayQuery(id, roundNumber), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapPost("/", async (AnalyzeOpponentDemoRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new AnalyzeOpponentDemoCommand(request.OpponentName, request.ObjectKey, request.FileName);
			var result = await sender.Send(new StartJobCommand(command), cancellationToken);
			return result.ToAcceptedJob();
		}).RequireAuthorization(policy => policy.RequireRole(coachOrManager));

		group.MapPut("/{id:guid}/team", async (Guid id, SetOpponentDemoTeamRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new SetOpponentDemoTeamCommand(id, request.Team), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole(coachOrManager));

		group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new DeleteOpponentDemoCommand(id), cancellationToken);
			return result.Match(_ => Results.NoContent(), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole(coachOrManager));

		group.MapPost("/faceit-download", async (DownloadOpponentDemosRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new DownloadOpponentDemosCommand(request.OpponentName, request.Maps, request.Count ?? 3);
			var result = await sender.Send(new StartJobCommand(command), cancellationToken);
			return result.ToAcceptedJob();
		}).RequireAuthorization(policy => policy.RequireRole(coachOrManager));
	}

	#endregion
}

/// <summary>Request body for POST /api/opponents/report/demos; <paramref name="FileName"/> is the original .dem name (FACEIT match recognition).</summary>
public record AnalyzeOpponentDemoRequest(string OpponentName, string ObjectKey, string? FileName = null);

/// <summary>Request body for PUT /api/opponents/report/demos/{id}/team — "A" (started T) or "B" (started CT).</summary>
public record SetOpponentDemoTeamRequest(string Team);

/// <summary>Request body for POST /api/opponents/report/demos/faceit-download; no maps means any pool map, count defaults to 3.</summary>
public record DownloadOpponentDemosRequest(string OpponentName, List<MapName>? Maps, int? Count);
