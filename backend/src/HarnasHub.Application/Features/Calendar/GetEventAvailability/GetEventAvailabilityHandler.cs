using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Calendar.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Calendar.GetEventAvailability;

/// <summary>Handles <see cref="GetEventAvailabilityQuery"/> by left-joining the roster with declared availability.</summary>
public class GetEventAvailabilityHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetEventAvailabilityQuery, ErrorOr<List<MemberAvailabilityDto>>>
{
    #region Public Methods

    public async Task<ErrorOr<List<MemberAvailabilityDto>>> Handle(
        GetEventAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        var eventExists = await dbContext.Events.AnyAsync(e => e.Id == request.EventId, cancellationToken);

        if (!eventExists)
        {
            return CalendarErrors.EventNotFound;
        }

        var members = await (
            from user in dbContext.Users
            join availability in dbContext.Availabilities.Where(a => a.EventId == request.EventId)
                on user.Id equals availability.UserId into userAvailability
            from availability in userAvailability.DefaultIfEmpty()
            orderby user.DisplayName
            select new MemberAvailabilityDto(
                user.Id,
                user.DisplayName,
                availability == null ? "NotSet" : availability.Status.ToString()))
            .ToListAsync(cancellationToken);

        return members;
    }

    #endregion
}
