using HarnasHub.Api.Common;
using HarnasHub.Application.Features.Jobs.StartJob;
using HarnasHub.Application.Features.OpponentReport.GetOpponentReport;
using HarnasHub.Application.Features.OpponentReport.LinkAndSyncOpponentFaceit;
using HarnasHub.Application.Features.OpponentReport.SyncOpponentFaceit;
using MediatR;

namespace HarnasHub.Api.Endpoints.OpponentReport;

/// <summary>Opponent report (FACEIT "them vs us") endpoints under /api/opponents/report.</summary>
public class OpponentReportEndpoints : IEndpoint
{
	#region Public Methods

	public static void MapEndpoints(IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/opponents/report").WithTags("OpponentReport").RequireAuthorization(AuthorizationPolicies.TeamMember);

		// Name in the query string, not the path: team names can contain "/" or "?" which would break a route segment.
		group.MapGet("/", async (string name, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetOpponentReportQuery(name), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapPut("/link", async (LinkOpponentFaceitRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			// Background job: links and immediately syncs (15–20 s) — the client follows it via /api/jobs/{id}.
			var result = await sender.Send(new StartJobCommand(new LinkAndSyncOpponentFaceitCommand(request.OpponentName, request.Source)), cancellationToken);
			return result.ToAcceptedJob();
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		group.MapPost("/refresh", async (string name, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new StartJobCommand(new SyncOpponentFaceitCommand(name, IsManual: true)), cancellationToken);
			return result.ToAcceptedJob();
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));
	}

	#endregion
}

/// <summary>Request body for PUT /api/opponents/report/link — <paramref name="Source"/> is a FACEIT team URL, match room URL or nickname list.</summary>
public record LinkOpponentFaceitRequest(string OpponentName, string Source);
