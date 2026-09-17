using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Availability.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Availability.UpdateVacation;

/// <summary>Handles <see cref="UpdateVacationCommand"/>.</summary>
public class UpdateVacationHandler(
	IApplicationDbContext dbContext,
	ICurrentUserService currentUser,
	IRealtimeNotifier realtimeNotifier) : IRequestHandler<UpdateVacationCommand, ErrorOr<VacationDto>>
{
	#region Public Methods

	public async Task<ErrorOr<VacationDto>> Handle(UpdateVacationCommand request, CancellationToken cancellationToken)
	{
		var vacation = await dbContext.Vacations
			.FirstOrDefaultAsync(v => v.Id == request.VacationId, cancellationToken);

		if (vacation is null)
		{
			return AvailabilityErrors.VacationNotFound;
		}

		var isOwner = vacation.UserId == currentUser.UserId;
		var isCoachOrManager = currentUser.Role == "Manager" || currentUser.IsCoach;

		if (!isOwner && !isCoachOrManager)
		{
			return AvailabilityErrors.NotYourVacation;
		}

		vacation.StartDate = request.StartDate;
		vacation.EndDate = request.EndDate;
		vacation.Reason = request.Reason;

		await dbContext.SaveChangesAsync(cancellationToken);

		await realtimeNotifier.NotifyAsync("availability-week", cancellationToken);
		await realtimeNotifier.NotifyAsync("dashboard", cancellationToken);

		return new VacationDto(vacation.Id, vacation.StartDate, vacation.EndDate, vacation.Reason);
	}

	#endregion
}
