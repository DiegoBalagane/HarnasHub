using HarnasHub.Api.Common;
using HarnasHub.Application.Features.Attendance.AddIncident;
using HarnasHub.Application.Features.Attendance.DeleteIncident;
using HarnasHub.Application.Features.Attendance.GetIncidents;
using HarnasHub.Application.Features.Attendance.GetSummary;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Api.Endpoints.Attendance;

/// <summary>Training lateness/absence tracking under /api/attendance — every team member can read, only Coach/Manager can log or remove entries.</summary>
public class AttendanceEndpoints : IEndpoint
{
	#region Public Methods

	public static void MapEndpoints(IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/attendance").WithTags("Attendance").RequireAuthorization(AuthorizationPolicies.TeamMember);

		group.MapGet("/summary", async (ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetSummaryQuery(), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapGet("/incidents", async (Guid? userId, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetIncidentsQuery(userId), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapPost("/incidents", async (AddIncidentRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new AddIncidentCommand(request.UserId, request.Type, request.OccurredOn, request.Note);
			var result = await sender.Send(command, cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		group.MapDelete("/incidents/{incidentId:guid}", async (Guid incidentId, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new DeleteIncidentCommand(incidentId), cancellationToken);
			return result.Match(success => Results.NoContent(), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));
	}

	#endregion
}

/// <summary>Request body for POST /api/attendance/incidents.</summary>
public record AddIncidentRequest(Guid UserId, AttendanceIncidentType Type, DateOnly OccurredOn, string? Note);
