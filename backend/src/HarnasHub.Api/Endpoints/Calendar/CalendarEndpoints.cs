using HarnasHub.Api.Common;
using HarnasHub.Application.Features.Calendar.CreateEvent;
using HarnasHub.Application.Features.Calendar.GetEventAvailability;
using HarnasHub.Application.Features.Calendar.GetUpcomingEvents;
using HarnasHub.Application.Features.Calendar.SetAvailability;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Api.Endpoints.Calendar;

/// <summary>Calendar events and availability endpoints under /api/calendar.</summary>
public class CalendarEndpoints : IEndpoint
{
    #region Public Methods

    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/calendar").WithTags("Calendar").RequireAuthorization();

        group.MapGet("/events", async (ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetUpcomingEventsQuery(), cancellationToken);
            return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
        });

        group.MapPost("/events", async (CreateEventRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new CreateEventCommand(request.Title, request.Type, request.StartsAtUtc, request.Location, request.Notes);
            var result = await sender.Send(command, cancellationToken);
            return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
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
public record CreateEventRequest(string Title, EventType Type, DateTime StartsAtUtc, string? Location, string? Notes);

/// <summary>Request body for POST /api/calendar/events/{eventId}/availability.</summary>
public record SetAvailabilityRequest(AvailabilityStatus Status);
