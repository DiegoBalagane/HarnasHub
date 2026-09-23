using HarnasHub.Api.Common;
using HarnasHub.Application.Features.Stats.AddPlayerStat;
using HarnasHub.Application.Features.Stats.GetMatchStats;
using HarnasHub.Application.Features.Stats.GetMyStatsHistory;
using HarnasHub.Application.Features.Stats.GetPlayerLeaderboard;
using HarnasHub.Application.Features.Stats.GetTeamTrend;
using HarnasHub.Application.Features.Stats.ImportStatsFromDemo;
using HarnasHub.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Http.Features;

namespace HarnasHub.Api.Endpoints.Stats;

/// <summary>Player and team statistics endpoints under /api/matches/{id}/stats and /api/stats.</summary>
public class StatsEndpoints : IEndpoint
{
	#region Public Methods

	public static void MapEndpoints(IEndpointRouteBuilder app)
	{
		var matchStats = app.MapGroup("/api/matches/{matchResultId:guid}/stats").WithTags("Stats")
			.RequireAuthorization(AuthorizationPolicies.TeamMember);

		matchStats.MapGet("/", async (Guid matchResultId, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetMatchStatsQuery(matchResultId), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		matchStats.MapPost("/", async (
			Guid matchResultId,
			AddPlayerStatRequest request,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var command = new AddPlayerStatCommand(
				matchResultId, request.UserId, request.Kills, request.Deaths, request.Assists,
				request.Adr, request.HeadshotPercentage, request.Rating,
				request.EntryKills, request.EntryDeaths, request.KastPercentage,
				request.MultiKill2K, request.MultiKill3K, request.MultiKill4K, request.MultiKill5K,
				request.UtilityDamage, request.FlashAssists);
			var result = await sender.Send(command, cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		var stats = app.MapGroup("/api/stats").WithTags("Stats").RequireAuthorization(AuthorizationPolicies.TeamMember);

		stats.MapGet("/mine", async (ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetMyStatsHistoryQuery(), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		stats.MapGet("/team-trend", async (ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetTeamTrendQuery(), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		stats.MapGet("/leaderboard", async (MatchCategory? category, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetPlayerLeaderboardQuery(category), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		// CS2 demos routinely run 100-300MB — raise Kestrel's per-request cap (default 30MB) for this endpoint only,
		// and stream straight from the multipart body into the parser instead of buffering into a byte[].
		stats.MapPost("/import-demo", async (HttpRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
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

			await using var demoStream = file.OpenReadStream();
			var result = await sender.Send(new ImportStatsFromDemoCommand(demoStream), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));
	}

	#endregion
}

/// <summary>Request body for POST /api/matches/{matchResultId}/stats. Everything from <paramref name="EntryKills"/> onward
/// is optional — the manual entry form never sends it, only a demo import does.</summary>
public record AddPlayerStatRequest(
	Guid UserId,
	int Kills,
	int Deaths,
	int Assists,
	double Adr,
	double HeadshotPercentage,
	double Rating,
	int? EntryKills = null,
	int? EntryDeaths = null,
	double? KastPercentage = null,
	int? MultiKill2K = null,
	int? MultiKill3K = null,
	int? MultiKill4K = null,
	int? MultiKill5K = null,
	int? UtilityDamage = null,
	int? FlashAssists = null);
