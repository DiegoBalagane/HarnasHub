using HarnasHub.Api.Common;
using HarnasHub.Application.Features.Leagues.CreateLeague;
using HarnasHub.Application.Features.Leagues.DeleteLeague;
using HarnasHub.Application.Features.Leagues.GetLeagues;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Api.Endpoints.Leagues;

/// <summary>League season grouping endpoints under /api/leagues.</summary>
public class LeaguesEndpoints : IEndpoint
{
	#region Public Methods

	public static void MapEndpoints(IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/leagues").WithTags("Leagues").RequireAuthorization(AuthorizationPolicies.TeamMember);

		group.MapGet("/", async (ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetLeaguesQuery(), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapPost("/", async (CreateLeagueRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new CreateLeagueCommand(request.Name, request.Season, request.Type), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		group.MapDelete("/{leagueId:guid}", async (Guid leagueId, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new DeleteLeagueCommand(leagueId), cancellationToken);
			return result.Match(success => Results.NoContent(), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));
	}

	#endregion
}

/// <summary>Request body for POST /api/leagues.</summary>
public record CreateLeagueRequest(string Name, string Season, LeagueType Type);
