using HarnasHub.Api.Common;
using HarnasHub.Application.Features.TrainingMaterials.AddMaterial;
using HarnasHub.Application.Features.TrainingMaterials.GetMaterials;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Api.Endpoints.TrainingMaterials;

/// <summary>Training material library endpoints under /api/training-materials.</summary>
public class TrainingMaterialsEndpoints : IEndpoint
{
	#region Public Methods

	public static void MapEndpoints(IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/training-materials").WithTags("TrainingMaterials")
			.RequireAuthorization(AuthorizationPolicies.TeamMember);

		group.MapGet("/", async (ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetMaterialsQuery(), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapPost("/", async (AddMaterialRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new AddMaterialCommand(request.Title, request.Url, request.Category, request.Description);
			var result = await sender.Send(command, cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));
	}

	#endregion
}

/// <summary>Request body for POST /api/training-materials.</summary>
public record AddMaterialRequest(string Title, string Url, MaterialCategory? Category, string? Description);
