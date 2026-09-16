using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Availability.Shared;
using HarnasHub.Core.Entities;
using MediatR;

namespace HarnasHub.Application.Features.Availability.SetVacation;

/// <summary>Handles <see cref="SetVacationCommand"/> by persisting the caller's time-off range.</summary>
public class SetVacationHandler(
	IApplicationDbContext dbContext,
	ICurrentUserService currentUser,
	IRealtimeNotifier realtimeNotifier) : IRequestHandler<SetVacationCommand, ErrorOr<VacationDto>>
{
	#region Public Methods

	public async Task<ErrorOr<VacationDto>> Handle(SetVacationCommand request, CancellationToken cancellationToken)
	{
		var vacation = new Vacation
		{
			Id = Guid.NewGuid(),
			UserId = currentUser.UserId,
			StartDate = request.StartDate,
			EndDate = request.EndDate,
			Reason = request.Reason,
			CreatedAtUtc = DateTime.UtcNow
		};

		dbContext.Vacations.Add(vacation);
		await dbContext.SaveChangesAsync(cancellationToken);

		await realtimeNotifier.NotifyAsync("availability-week", cancellationToken);
		await realtimeNotifier.NotifyAsync("dashboard", cancellationToken);

		return new VacationDto(vacation.Id, vacation.StartDate, vacation.EndDate, vacation.Reason);
	}

	#endregion
}
