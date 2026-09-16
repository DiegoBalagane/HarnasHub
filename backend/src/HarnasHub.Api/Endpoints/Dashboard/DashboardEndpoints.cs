using HarnasHub.Api.Common;
using HarnasHub.Application.Features.Dashboard.GetDashboardSummary;
using MediatR;

namespace HarnasHub.Api.Endpoints.Dashboard;

/// <summary>Dashboard summary endpoint under /api/dashboard.</summary>
public class DashboardEndpoints : IEndpoint
{
	#region Public Methods

	public static void MapEndpoints(IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/dashboard").WithTags("Dashboard").RequireAuthorization(AuthorizationPolicies.TeamMember);

		group.MapGet("/", async (ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetDashboardSummaryQuery(), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});
	}

	#endregion
}
