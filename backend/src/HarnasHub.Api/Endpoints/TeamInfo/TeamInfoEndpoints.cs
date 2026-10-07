using HarnasHub.Api.Common;
using HarnasHub.Application.Features.TeamInfo.CreateTeamInfoEntry;
using HarnasHub.Application.Features.TeamInfo.DeleteTeamInfoEntry;
using HarnasHub.Application.Features.TeamInfo.GetTeamInfo;
using HarnasHub.Application.Features.TeamInfo.ReorderTeamInfoEntries;
using HarnasHub.Application.Features.TeamInfo.UpdateTeamInfoEntry;
using MediatR;

namespace HarnasHub.Api.Endpoints.TeamInfo;

/// <summary>Team info endpoints under /api/team-info: constant technical facts (Discord, servers, configs); read by members, edited by Coach/Manager.</summary>
public class TeamInfoEndpoints : IEndpoint
{
	#region Public Methods

	public static void MapEndpoints(IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/team-info").WithTags("TeamInfo").RequireAuthorization(AuthorizationPolicies.TeamMember);

		group.MapGet("/", async (ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetTeamInfoQuery(), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapPost("/", async (TeamInfoEntryRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new CreateTeamInfoEntryCommand(request.Category, request.Title, request.Value, request.IsSecret), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		group.MapPut("/{id:guid}", async (Guid id, TeamInfoEntryRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new UpdateTeamInfoEntryCommand(id, request.Category, request.Title, request.Value, request.IsSecret), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new DeleteTeamInfoEntryCommand(id), cancellationToken);
			return result.Match(success => Results.NoContent(), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		group.MapPut("/order", async (ReorderTeamInfoRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new ReorderTeamInfoEntriesCommand(request.OrderedIds), cancellationToken);
			return result.Match(success => Results.NoContent(), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));
	}

	#endregion
}

/// <summary>Request body for POST/PUT /api/team-info.</summary>
public record TeamInfoEntryRequest(string Category, string Title, string Value, bool IsSecret);

/// <summary>Request body for PUT /api/team-info/order: the ids of one category in their new order.</summary>
public record ReorderTeamInfoRequest(List<Guid> OrderedIds);
