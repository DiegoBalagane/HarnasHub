#region Usings

using HarnasHub.Api.Common;
using HarnasHub.Application.Features.Jobs.StartJob;
using HarnasHub.Application.Features.MatchAnalysis.AttachDemoToResult;
using HarnasHub.Application.Features.MatchAnalysis.ExcludePlayer;
using HarnasHub.Application.Features.MatchAnalysis.GetMapAnalytics;
using HarnasHub.Application.Features.MatchAnalysis.GetMatchAnalysis;
using HarnasHub.Application.Features.MatchAnalysis.GetMatchInsights;
using HarnasHub.Application.Features.MatchAnalysis.GetMatchTimeline;
using HarnasHub.Application.Features.MatchAnalysis.GetRoundReplay;
using HarnasHub.Application.Features.MatchAnalysis.IncludePlayer;
using HarnasHub.Application.Features.Tactics.GetMatchTacticMatches;
using HarnasHub.Core.Enums;
using MediatR;

#endregion

namespace HarnasHub.Api.Endpoints.MatchAnalysis;

/// <summary>Match timeline, insights and demo attachment endpoints under /api/results/{matchResultId}.</summary>
public class MatchAnalysisEndpoints : IEndpoint
{
	#region Public Methods

	/// <summary>Maps the timeline/insights reads (team members) and the demo attach (Coach/Manager).</summary>
	public static void MapEndpoints(IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/results/{matchResultId:guid}").WithTags("MatchAnalysis").RequireAuthorization(AuthorizationPolicies.TeamMember);

		group.MapGet("/timeline", async (Guid matchResultId, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetMatchTimelineQuery(matchResultId), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapGet("/analysis", async (Guid matchResultId, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetMatchAnalysisQuery(matchResultId), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapGet("/insights", async (Guid matchResultId, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetMatchInsightsQuery(matchResultId), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapGet("/rounds/{roundNumber:int}/replay", async (Guid matchResultId, int roundNumber, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetRoundReplayQuery(matchResultId, roundNumber), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapGet("/tactic-matches", async (Guid matchResultId, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetMatchTacticMatchesQuery(matchResultId), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapPost("/analysis/exclude", async (Guid matchResultId, PlayerAnalysisRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			if (!long.TryParse(request.SteamId64, out var steamId))
			{
				return Results.BadRequest();
			}

			var result = await sender.Send(new ExcludePlayerCommand(matchResultId, steamId), cancellationToken);
			return result.Match(_ => Results.NoContent(), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		group.MapPost("/analysis/include", async (Guid matchResultId, PlayerAnalysisRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			if (!long.TryParse(request.SteamId64, out var steamId))
			{
				return Results.BadRequest();
			}

			var result = await sender.Send(new IncludePlayerCommand(matchResultId, steamId), cancellationToken);
			return result.Match(_ => Results.NoContent(), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		app.MapGet("/api/maps/{map}/analytics", async (MapName map, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetMapAnalyticsQuery(map), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).WithTags("MatchAnalysis").RequireAuthorization(AuthorizationPolicies.TeamMember);

		// The demo goes browser → presigned URL (POST /api/results/analyze-demo/presign) first; this only names the object.
		group.MapPost("/demo", async (Guid matchResultId, AttachDemoRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new StartJobCommand(new AttachDemoToResultCommand(matchResultId, request.ObjectKey)), cancellationToken);
			return result.ToAcceptedJob();
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));
	}

	#endregion
}

/// <summary>Request body for POST /api/results/{matchResultId}/demo.</summary>
public record AttachDemoRequest(string ObjectKey);

/// <summary>Request body for POST /api/results/{matchResultId}/analysis/exclude and /include (SteamID64 as a string).</summary>
public record PlayerAnalysisRequest(string SteamId64);
