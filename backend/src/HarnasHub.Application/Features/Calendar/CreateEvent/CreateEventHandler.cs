using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Calendar.Shared;
using HarnasHub.Core.Entities;
using MediatR;

namespace HarnasHub.Application.Features.Calendar.CreateEvent;

/// <summary>Handles <see cref="CreateEventCommand"/> by persisting the new event.</summary>
public class CreateEventHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser)
    : IRequestHandler<CreateEventCommand, ErrorOr<EventDto>>
{
    #region Public Methods

    public async Task<ErrorOr<EventDto>> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var calendarEvent = new Event
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Type = request.Type,
            StartsAtUtc = request.StartsAtUtc,
            Location = request.Location,
            Notes = request.Notes,
            CreatedByUserId = currentUser.UserId,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Events.Add(calendarEvent);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new EventDto(
            calendarEvent.Id,
            calendarEvent.Title,
            calendarEvent.Type.ToString(),
            calendarEvent.StartsAtUtc,
            calendarEvent.Location,
            calendarEvent.Notes);
    }

    #endregion
}
