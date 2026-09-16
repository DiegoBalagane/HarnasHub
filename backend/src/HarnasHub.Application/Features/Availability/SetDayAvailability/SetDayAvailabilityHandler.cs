using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Availability.SetDayAvailability;

/// <summary>Handles <see cref="SetDayAvailabilityCommand"/> by upserting the caller's row for that day.</summary>
public class SetDayAvailabilityHandler(
	IApplicationDbContext dbContext,
	ICurrentUserService currentUser,
	IRealtimeNotifier realtimeNotifier) : IRequestHandler<SetDayAvailabilityCommand, ErrorOr<Success>>
{
	#region Public Methods

	public async Task<ErrorOr<Success>> Handle(SetDayAvailabilityCommand request, CancellationToken cancellationToken)
	{
		var userId = currentUser.UserId;
		var isPartial = request.Status == DayAvailabilityStatus.PartiallyAvailable;

		// Hours only carry meaning for a partial day — drop them otherwise so stale values can't linger after a status change.
		var availableFrom = isPartial ? request.AvailableFromLocal : null;
		var availableTo = isPartial ? request.AvailableToLocal : null;

		var day = await dbContext.PlayerAvailabilityDays
			.FirstOrDefaultAsync(d => d.UserId == userId && d.Date == request.Date, cancellationToken);

		if (day is null)
		{
			dbContext.PlayerAvailabilityDays.Add(new PlayerAvailabilityDay
			{
				Id = Guid.NewGuid(),
				UserId = userId,
				Date = request.Date,
				Status = request.Status,
				AvailableFromLocal = availableFrom,
				AvailableToLocal = availableTo,
				Note = request.Note,
				UpdatedAtUtc = DateTime.UtcNow
			});
		}
		else
		{
			day.Status = request.Status;
			day.AvailableFromLocal = availableFrom;
			day.AvailableToLocal = availableTo;
			day.Note = request.Note;
			day.UpdatedAtUtc = DateTime.UtcNow;
		}

		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("availability-week", cancellationToken);
		await realtimeNotifier.NotifyAsync("dashboard", cancellationToken);

		return Result.Success;
	}

	#endregion
}
