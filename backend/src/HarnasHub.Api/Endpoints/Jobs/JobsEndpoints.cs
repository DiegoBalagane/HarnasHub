#region Usings

using HarnasHub.Api.Common;
using HarnasHub.Application.Features.Jobs.GetJob;
using MediatR;

#endregion

namespace HarnasHub.Api.Endpoints.Jobs;

/// <summary>Background job status endpoints under /api/jobs.</summary>
public class JobsEndpoints : IEndpoint
{
	#region Public Methods

	/// <summary>Maps <c>GET /api/jobs/{jobId}</c> — status, progress, stage and (once done) result or error of a job.</summary>
	public static void MapEndpoints(IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/jobs").WithTags("Jobs").RequireAuthorization(AuthorizationPolicies.TeamMember);

		group.MapGet("/{jobId:guid}", async (Guid jobId, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetJobQuery(jobId), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});
	}

	#endregion
}
