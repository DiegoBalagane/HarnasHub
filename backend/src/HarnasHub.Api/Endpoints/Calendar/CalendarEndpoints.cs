using HarnasHub.Api.Common;
using HarnasHub.Application.Features.Calendar.CreateEvent;
using HarnasHub.Application.Features.Calendar.DeleteEvent;
using HarnasHub.Application.Features.Calendar.GetEventAvailability;
using HarnasHub.Application.Features.Calendar.GetUpcomingEvents;
using HarnasHub.Application.Features.Calendar.SetAvailability;
using HarnasHub.Application.Features.Calendar.UpdateEvent;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Api.Endpoints.Calendar;

/// <summary>Calendar events and availability endpoints under /api/calendar.</summary>
public class CalendarEndpoints : IEndpoint
{
	#region Public Methods

	public static void MapEndpoints(IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/calendar").WithTags("Calendar").RequireAuthorization(AuthorizationPolicies.TeamMember);

		group.MapGet("/events", async (bool? includePast, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetUpcomingEventsQuery(includePast ?? false), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapPost("/events", async (CreateEventRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new CreateEventCommand(request.Title, request.Type, request.StartsAtUtc, request.EndsAtUtc, request.Location, request.Url, request.Notes);
			var result = await sender.Send(command, cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		group.MapPut("/events/{eventId:guid}", async (
			Guid eventId,
			UpdateEventRequest request,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var command = new UpdateEventCommand(
				eventId,
				request.Title,
				request.Type,
				request.StartsAtUtc,
				request.EndsAtUtc,
				request.Location,
				request.Url,
				request.Notes);
			var result = await sender.Send(command, cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		group.MapDelete("/events/{eventId:guid}", async (
			Guid eventId,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new DeleteEventCommand(eventId), cancellationToken);
			return result.Match(success => Results.NoContent(), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		group.MapGet("/events/{eventId:guid}/availability", async (
			Guid eventId,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetEventAvailabilityQuery(eventId), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapPost("/events/{eventId:guid}/availability", async (
			Guid eventId,
			SetAvailabilityRequest request,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var command = new SetAvailabilityCommand(eventId, request.Status);
			var result = await sender.Send(command, cancellationToken);
			return result.Match(success => Results.NoContent(), errors => errors.ToProblemResult());
		});
	}

	#endregion
}

/// <summary>Request body for POST /api/calendar/events.</summary>
public record CreateEventRequest(string Title, EventType Type, DateTime StartsAtUtc, DateTime? EndsAtUtc, string? Location, string? Url, string? Notes);

/// <summary>Request body for PUT /api/calendar/events/{eventId}.</summary>
public record UpdateEventRequest(string Title, EventType Type, DateTime StartsAtUtc, DateTime? EndsAtUtc, string? Location, string? Url, string? Notes);

/// <summary>Request body for POST /api/calendar/events/{eventId}/availability.</summary>
public record SetAvailabilityRequest(AvailabilityStatus Status);
