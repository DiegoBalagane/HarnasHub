using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Availability.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Availability.GetVacations;

/// <summary>Handles <see cref="GetVacationsQuery"/> by listing the caller's vacations.</summary>
public class GetVacationsHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser)
	: IRequestHandler<GetVacationsQuery, ErrorOr<List<VacationDto>>>
{
	#region Public Methods

	public async Task<ErrorOr<List<VacationDto>>> Handle(GetVacationsQuery request, CancellationToken cancellationToken)
	{
		var userId = currentUser.UserId;

		var vacations = await dbContext.Vacations
			.Where(vacation => vacation.UserId == userId)
			.OrderByDescending(vacation => vacation.StartDate)
			.Select(vacation => new VacationDto(vacation.Id, vacation.StartDate, vacation.EndDate, vacation.Reason))
			.ToListAsync(cancellationToken);

		return vacations;
	}

	#endregion
}
