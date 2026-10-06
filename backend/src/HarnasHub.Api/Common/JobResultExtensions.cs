#region Usings

using ErrorOr;
using HarnasHub.Application.Features.Jobs.Shared;

#endregion

namespace HarnasHub.Api.Common;

/// <summary>Maps the result of starting a background job to an HTTP response.</summary>
public static class JobResultExtensions
{
	#region Public Methods

	/// <summary>202 Accepted with <c>{ jobId }</c> and a Location pointing at <c>GET /api/jobs/{id}</c>, or the problem result.</summary>
	public static IResult ToAcceptedJob(this ErrorOr<JobAcceptedDto> result) =>
		result.Match(
			accepted => Results.Accepted($"/api/jobs/{accepted.JobId}", accepted),
			errors => errors.ToProblemResult());

	#endregion
}
