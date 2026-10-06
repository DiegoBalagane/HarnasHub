using HarnasHub.Api.Common;
using HarnasHub.Application.Features.Veto.GetEventVeto;
using HarnasHub.Application.Features.Veto.GetVetoSuggestion;
using HarnasHub.Application.Features.Veto.SetEventVeto;
using MediatR;

namespace HarnasHub.Api.Endpoints.Veto;

/// <summary>Map veto endpoints under /api/veto: the suggestion against an opponent and the veto recorded per event.</summary>
public class VetoEndpoints : IEndpoint
{
	#region Public Methods

	public static void MapEndpoints(IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/veto").WithTags("Veto").RequireAuthorization(AuthorizationPolicies.TeamMember);

		group.MapGet("/suggestion", async (string opponent, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetVetoSuggestionQuery(opponent), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapGet("/events/{eventId:guid}", async (Guid eventId, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetEventVetoQuery(eventId), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapPut("/events/{eventId:guid}", async (
			Guid eventId,
			SetEventVetoRequest request,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new SetEventVetoCommand(eventId, request.Steps), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));
	}

	#endregion
}

/// <summary>Request body for PUT /api/veto/events/{eventId} — the full veto in order; an empty list clears it.</summary>
public record SetEventVetoRequest(List<VetoStepInput> Steps);
