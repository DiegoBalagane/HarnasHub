using HarnasHub.Api.Common;
using HarnasHub.Application.Features.Admin.GetAdminStatus;
using MediatR;

namespace HarnasHub.Api.Endpoints.Admin;

/// <summary>Manager-only administration endpoints under /api/admin.</summary>
public class AdminEndpoints : IEndpoint
{
	#region Public Methods

	public static void MapEndpoints(IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/admin").WithTags("Admin").RequireAuthorization(policy => policy.RequireRole("Manager"));

		group.MapGet("/status", async (ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetAdminStatusQuery(), cancellationToken);

			return result.Match(
				success => Results.Ok(success),
				errors => errors.ToProblemResult());
		});
	}

	#endregion
}
