using HarnasHub.Api.Common;
using HarnasHub.Application.Features.Availability.DeleteVacation;
using HarnasHub.Application.Features.Availability.GetVacations;
using HarnasHub.Application.Features.Availability.GetWeekAvailability;
using HarnasHub.Application.Features.Availability.SetDayAvailability;
using HarnasHub.Application.Features.Availability.SetVacation;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Api.Endpoints.Availability;

/// <summary>Weekly availability and vacation endpoints under /api/availability.</summary>
public class AvailabilityEndpoints : IEndpoint
{
	#region Public Methods

	public static void MapEndpoints(IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/availability").WithTags("Availability").RequireAuthorization(AuthorizationPolicies.TeamMember);

		group.MapGet("/week", async (DateOnly weekStart, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetWeekAvailabilityQuery(weekStart), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapPost("/day", async (SetDayAvailabilityRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new SetDayAvailabilityCommand(
				request.Date,
				request.Status,
				request.AvailableFromLocal,
				request.AvailableToLocal,
				request.Note);
			var result = await sender.Send(command, cancellationToken);
			return result.Match(success => Results.NoContent(), errors => errors.ToProblemResult());
		});

		group.MapGet("/vacations", async (ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetVacationsQuery(), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapPost("/vacations", async (SetVacationRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new SetVacationCommand(request.StartDate, request.EndDate, request.Reason);
			var result = await sender.Send(command, cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapDelete("/vacations/{vacationId:guid}", async (
			Guid vacationId,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new DeleteVacationCommand(vacationId), cancellationToken);
			return result.Match(success => Results.NoContent(), errors => errors.ToProblemResult());
		});
	}

	#endregion
}

/// <summary>Request body for POST /api/availability/day.</summary>
public record SetDayAvailabilityRequest(
	DateOnly Date,
	DayAvailabilityStatus Status,
	TimeOnly? AvailableFromLocal,
	TimeOnly? AvailableToLocal,
	string? Note);

/// <summary>Request body for POST /api/availability/vacations.</summary>
public record SetVacationRequest(DateOnly StartDate, DateOnly EndDate, string? Reason);
