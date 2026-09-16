using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Availability.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Availability.DeleteVacation;

/// <summary>Handles <see cref="DeleteVacationCommand"/>.</summary>
public class DeleteVacationHandler(
	IApplicationDbContext dbContext,
	ICurrentUserService currentUser,
	IRealtimeNotifier realtimeNotifier) : IRequestHandler<DeleteVacationCommand, ErrorOr<Success>>
{
	#region Public Methods

	public async Task<ErrorOr<Success>> Handle(DeleteVacationCommand request, CancellationToken cancellationToken)
	{
		var vacation = await dbContext.Vacations
			.FirstOrDefaultAsync(v => v.Id == request.VacationId, cancellationToken);

		if (vacation is null)
		{
			return AvailabilityErrors.VacationNotFound;
		}

		var isOwner = vacation.UserId == currentUser.UserId;
		var isCoachOrManager = currentUser.Role is "Coach" or "Manager";

		if (!isOwner && !isCoachOrManager)
		{
			return AvailabilityErrors.NotYourVacation;
		}

		dbContext.Vacations.Remove(vacation);
		await dbContext.SaveChangesAsync(cancellationToken);

		await realtimeNotifier.NotifyAsync("availability-week", cancellationToken);
		await realtimeNotifier.NotifyAsync("dashboard", cancellationToken);

		return Result.Success;
	}

	#endregion
}
