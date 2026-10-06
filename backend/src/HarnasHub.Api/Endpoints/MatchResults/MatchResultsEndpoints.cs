using HarnasHub.Api.Common;
using HarnasHub.Application.Features.Jobs.StartJob;
using HarnasHub.Application.Features.Results.AddResult;
using HarnasHub.Application.Features.Results.AnalyzeDemoFromStorage;
using HarnasHub.Application.Features.Results.AnalyzeUploadedDemo;
using HarnasHub.Application.Features.Results.DeleteResult;
using HarnasHub.Application.Features.Results.GetResults;
using HarnasHub.Application.Features.Results.PresignDemoUpload;
using HarnasHub.Application.Features.Results.Shared;
using HarnasHub.Application.Features.Results.UpdateResult;
using HarnasHub.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Http.Features;

namespace HarnasHub.Api.Endpoints.MatchResults;

/// <summary>Match/scrim result endpoints under /api/results.</summary>
public class MatchResultsEndpoints : IEndpoint
{
	#region Public Methods

	public static void MapEndpoints(IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/results").WithTags("Results").RequireAuthorization(AuthorizationPolicies.TeamMember);

		group.MapGet("/", async (ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetResultsQuery(), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapPost("/", async (AddResultRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new AddResultCommand(
				request.Opponent, request.OurScore, request.OpponentScore, request.MapName, request.DemoUrl, request.Notes,
				request.PlayedAtUtc, request.Category, request.TournamentId, request.LeagueId,
				request.DemoRoundsPlayed, request.DemoPlayers, request.OurTeamSteamIds, request.PendingTimelineKey);
			var result = await sender.Send(command, cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		group.MapPatch("/{matchResultId:guid}", async (Guid matchResultId, UpdateResultRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new UpdateResultCommand(
				matchResultId, request.Opponent, request.OurScore, request.OpponentScore, request.MapName, request.DemoUrl,
				request.Notes, request.PlayedAtUtc, request.Category, request.TournamentId, request.LeagueId);
			var result = await sender.Send(command, cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		group.MapDelete("/{matchResultId:guid}", async (Guid matchResultId, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new DeleteResultCommand(matchResultId), cancellationToken);
			return result.Match(_ => Results.NoContent(), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		// A separate step from creating the result: parses the demo and previews the map/score/roster-suggested team
		// split so the coach can review and correct it before anything is saved (only the background job row is stored).
		group.MapPost("/analyze-demo", async (HttpRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			// CS2 demos routinely run 100-300MB — raise Kestrel's per-request cap (default 30MB) for this endpoint only.
			var sizeFeature = request.HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();
			if (sizeFeature is { IsReadOnly: false })
			{
				sizeFeature.MaxRequestBodySize = 320_000_000;
			}

			if (!request.HasFormContentType)
			{
				return Results.BadRequest("Oczekiwano pliku demki jako multipart/form-data.");
			}

			// ReadFormAsync's own MultipartBodyLengthLimit defaults to 128MB regardless of the Kestrel
			// request-body cap raised above — must be set separately or large demos are rejected mid-read.
			var formOptions = new FormOptions { MultipartBodyLengthLimit = 320_000_000 };
			var form = await request.ReadFormAsync(formOptions, cancellationToken);
			var file = form.Files.GetFile("demo");

			if (file is null || file.Length == 0)
			{
				return Results.BadRequest("Brak pliku demki.");
			}

			// Parsing runs as a background job (202 + job id), so the upload is buffered to a server-named temp file the job
			// reads and deletes — the request's own form file is gone as soon as this request ends.
			var tempPath = UploadedDemoFiles.NewTempPath();
			await using (var demoStream = file.OpenReadStream())
			await using (var tempFile = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
			{
				await demoStream.CopyToAsync(tempFile, cancellationToken);
			}

			var result = await sender.Send(new StartJobCommand(new AnalyzeUploadedDemoCommand(tempPath, file.FileName)), cancellationToken);
			if (result.IsError)
			{
				UploadedDemoFiles.TryDelete(tempPath);
			}

			return result.ToAcceptedJob();
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		// For demos too large for the above (the hosting platform's own edge proxy has its own size limit that no
		// setting in this app can raise) — the browser uploads straight to object storage instead of through here.
		group.MapPost("/analyze-demo/presign", async (ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new PresignDemoUploadCommand(), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		group.MapPost("/analyze-demo/from-storage", async (
			AnalyzeDemoFromStorageRequest request,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new StartJobCommand(new AnalyzeDemoFromStorageCommand(request.ObjectKey, request.FileName)), cancellationToken);
			return result.ToAcceptedJob();
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));
	}

	#endregion
}

/// <summary>Request body for POST /api/results. <see cref="DemoRoundsPlayed"/>/<see cref="DemoPlayers"/> carry an
/// earlier <c>POST /api/results/analyze-demo</c> response's raw player totals back in, so stats can be imported
/// without re-uploading the demo.</summary>
public record AddResultRequest(
	string Opponent,
	int? OurScore,
	int? OpponentScore,
	string? MapName,
	string? DemoUrl,
	string? Notes,
	DateTime PlayedAtUtc,
	MatchCategory Category,
	Guid? TournamentId,
	Guid? LeagueId,
	int? DemoRoundsPlayed,
	IReadOnlyList<AnalyzedDemoPlayerDto>? DemoPlayers,
	IReadOnlyList<string>? OurTeamSteamIds,
	string? PendingTimelineKey = null);

/// <summary>Request body for POST /api/results/analyze-demo/from-storage; <paramref name="FileName"/> is the original .dem
/// name, used to recognise a FACEIT match.</summary>
public record AnalyzeDemoFromStorageRequest(string ObjectKey, string? FileName = null);

/// <summary>Request body for PATCH /api/results/{matchResultId}.</summary>
public record UpdateResultRequest(
	string Opponent,
	int OurScore,
	int OpponentScore,
	string? MapName,
	string? DemoUrl,
	string? Notes,
	DateTime PlayedAtUtc,
	MatchCategory Category,
	Guid? TournamentId,
	Guid? LeagueId);
