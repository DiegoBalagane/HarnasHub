using HarnasHub.Api.Common;
using HarnasHub.Application.Features.Stats.AddPlayerStat;
using HarnasHub.Application.Features.Stats.GetMatchStats;
using HarnasHub.Application.Features.Stats.GetMyStatsHistory;
using HarnasHub.Application.Features.Stats.GetTeamTrend;
using MediatR;

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
				request.Adr, request.HeadshotPercentage, request.Rating);
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
	}

	#endregion
}

/// <summary>Request body for POST /api/matches/{matchResultId}/stats.</summary>
public record AddPlayerStatRequest(Guid UserId, int Kills, int Deaths, int Assists, double Adr, double HeadshotPercentage, double Rating);
