using HarnasHub.Api.Common;
using HarnasHub.Application.Features.Results.AddResult;
using HarnasHub.Application.Features.Results.GetResults;
using MediatR;

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
				request.Opponent, request.OurScore, request.OpponentScore, request.MapName, request.DemoUrl, request.Notes, request.PlayedAtUtc);
			var result = await sender.Send(command, cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));
	}

	#endregion
}

/// <summary>Request body for POST /api/results.</summary>
public record AddResultRequest(
	string Opponent,
	int OurScore,
	int OpponentScore,
	string? MapName,
	string? DemoUrl,
	string? Notes,
	DateTime PlayedAtUtc);
