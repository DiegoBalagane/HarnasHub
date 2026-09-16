using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Calendar.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

// Aliased because the Features.Availability namespace would otherwise shadow the entity name here.
using AvailabilityEntity = HarnasHub.Core.Entities.Availability;

namespace HarnasHub.Application.Features.Calendar.SetAvailability;

/// <summary>Handles <see cref="SetAvailabilityCommand"/> by upserting the caller's availability row.</summary>
public class SetAvailabilityHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<SetAvailabilityCommand, ErrorOr<Success>>
{
	#region Public Methods

	public async Task<ErrorOr<Success>> Handle(SetAvailabilityCommand request, CancellationToken cancellationToken)
	{
		var eventExists = await dbContext.Events.AnyAsync(e => e.Id == request.EventId, cancellationToken);

		if (!eventExists)
		{
			return CalendarErrors.EventNotFound;
		}

		var userId = currentUser.UserId;

		var availability = await dbContext.Availabilities
			.FirstOrDefaultAsync(a => a.EventId == request.EventId && a.UserId == userId, cancellationToken);

		if (availability is null)
		{
			dbContext.Availabilities.Add(new AvailabilityEntity
			{
				Id = Guid.NewGuid(),
				EventId = request.EventId,
				UserId = userId,
				Status = request.Status,
				UpdatedAtUtc = DateTime.UtcNow
			});
		}
		else
		{
			availability.Status = request.Status;
			availability.UpdatedAtUtc = DateTime.UtcNow;
		}

		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync($"availability:{request.EventId}", cancellationToken);

		return Result.Success;
	}

	#endregion
}
