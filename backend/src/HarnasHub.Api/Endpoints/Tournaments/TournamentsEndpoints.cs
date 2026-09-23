using HarnasHub.Api.Common;
using HarnasHub.Application.Features.Tournaments.CreateTournament;
using HarnasHub.Application.Features.Tournaments.DeleteTournament;
using HarnasHub.Application.Features.Tournaments.GetTournaments;
using MediatR;

namespace HarnasHub.Api.Endpoints.Tournaments;

/// <summary>Tournament grouping endpoints under /api/tournaments.</summary>
public class TournamentsEndpoints : IEndpoint
{
	#region Public Methods

	public static void MapEndpoints(IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/tournaments").WithTags("Tournaments").RequireAuthorization(AuthorizationPolicies.TeamMember);

		group.MapGet("/", async (ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetTournamentsQuery(), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapPost("/", async (CreateTournamentRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new CreateTournamentCommand(request.Name), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		group.MapDelete("/{tournamentId:guid}", async (Guid tournamentId, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new DeleteTournamentCommand(tournamentId), cancellationToken);
			return result.Match(success => Results.NoContent(), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));
	}

	#endregion
}

/// <summary>Request body for POST /api/tournaments.</summary>
public record CreateTournamentRequest(string Name);
