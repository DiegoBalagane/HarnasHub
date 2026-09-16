using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Availability.Shared;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Availability.GetWeekAvailability;

/// <summary>Handles <see cref="GetWeekAvailabilityQuery"/> by combining daily declarations with vacations, which win.</summary>
public class GetWeekAvailabilityHandler(IApplicationDbContext dbContext)
	: IRequestHandler<GetWeekAvailabilityQuery, ErrorOr<WeekAvailabilityDto>>
{
	#region Private Fields

	private const int DaysInWeek = 7;

	#endregion

	#region Public Methods

	public async Task<ErrorOr<WeekAvailabilityDto>> Handle(
		GetWeekAvailabilityQuery request,
		CancellationToken cancellationToken)
	{
		var weekStart = request.WeekStart;
		var weekEnd = weekStart.AddDays(DaysInWeek - 1);

		// Stand-ins fill a temporary gap and never show up in the availability calendar; everyone else does,
		// with the main five sorted above the bench (and anyone with no slot assigned yet last).
		var members = await dbContext.Users
			.Where(user => user.RosterSlot != RosterSlot.StandIn)
			.OrderBy(user => user.RosterSlot == RosterSlot.Main ? 0 : user.RosterSlot == RosterSlot.Bench ? 1 : 2)
			.ThenBy(user => user.DisplayName)
			.Select(user => new { user.Id, user.DisplayName, user.InGameNickname, user.TeamRole, user.RosterSlot })
			.ToListAsync(cancellationToken);

		var declaredDays = await dbContext.PlayerAvailabilityDays
			.Where(day => day.Date >= weekStart && day.Date <= weekEnd)
			.ToListAsync(cancellationToken);

		var vacations = await dbContext.Vacations
			.Where(vacation => vacation.StartDate <= weekEnd && vacation.EndDate >= weekStart)
			.ToListAsync(cancellationToken);

		var declaredByUserAndDate = declaredDays.ToDictionary(day => (day.UserId, day.Date));
		var vacationsByUser = vacations.ToLookup(vacation => vacation.UserId);

		var rows = members
			.Select(member => new MemberWeekDto(
				member.Id,
				member.DisplayName,
				member.InGameNickname,
				member.TeamRole?.ToString(),
				member.RosterSlot?.ToString(),
				BuildDays(weekStart, member.Id, declaredByUserAndDate, vacationsByUser)))
			.ToList();

		return new WeekAvailabilityDto(rows);
	}

	#endregion

	#region Private Methods

	private static List<DayEntryDto> BuildDays(
		DateOnly weekStart,
		Guid userId,
		Dictionary<(Guid UserId, DateOnly Date), PlayerAvailabilityDay> declaredByUserAndDate,
		ILookup<Guid, Vacation> vacationsByUser)
	{
		var days = new List<DayEntryDto>(DaysInWeek);

		for (var offset = 0; offset < DaysInWeek; offset++)
		{
			var date = weekStart.AddDays(offset);
			var effective = EffectiveAvailabilityCalculator.Resolve(
				userId,
				date,
				declaredByUserAndDate,
				vacationsByUser);

			days.Add(new DayEntryDto(
				date,
				effective.Status,
				effective.From,
				effective.To,
				effective.IsVacation,
				effective.Note));
		}

		return days;
	}

	#endregion
}
