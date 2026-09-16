using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Availability.Shared;
using HarnasHub.Application.Features.Calendar.Shared;
using HarnasHub.Application.Features.Dashboard.Shared;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Dashboard.GetDashboardSummary;

/// <summary>Identity of one team member while the daily status lists are being built.</summary>
internal readonly record struct DashboardMember(Guid Id, string DisplayName, string? TeamRole);

/// <summary>Handles <see cref="GetDashboardSummaryQuery"/>.</summary>
public class GetDashboardSummaryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser)
	: IRequestHandler<GetDashboardSummaryQuery, ErrorOr<DashboardSummaryDto>>
{
	#region Public Methods

	public async Task<ErrorOr<DashboardSummaryDto>> Handle(
		GetDashboardSummaryQuery request,
		CancellationToken cancellationToken)
	{
		var now = DateTime.UtcNow;

		var nextEvent = await dbContext.Events
			.Where(e => e.StartsAtUtc >= now)
			.OrderBy(e => e.StartsAtUtc)
			.Select(e => new EventDto(e.Id, e.Title, e.Type.ToString(), e.StartsAtUtc, e.Location, e.Notes))
			.FirstOrDefaultAsync(cancellationToken);

		var userId = currentUser.UserId;

		var openTaskCount = await dbContext.Tasks
			.CountAsync(t => t.AssignedToUserId == userId && t.Status == TaskItemStatus.Todo, cancellationToken);

		var today = DateOnly.FromDateTime(now);
		var tomorrow = today.AddDays(1);

		var members = (await dbContext.Users
				.OrderBy(user => user.DisplayName)
				.Select(user => new { user.Id, user.DisplayName, user.TeamRole })
				.ToListAsync(cancellationToken))
			.Select(user => new DashboardMember(user.Id, user.DisplayName, user.TeamRole?.ToString()))
			.ToList();

		var declaredDays = await dbContext.PlayerAvailabilityDays
			.Where(day => day.Date == today || day.Date == tomorrow)
			.ToListAsync(cancellationToken);

		var vacations = await dbContext.Vacations
			.Where(vacation => vacation.StartDate <= tomorrow && vacation.EndDate >= today)
			.ToListAsync(cancellationToken);

		var declaredByUserAndDate = declaredDays.ToDictionary(day => (day.UserId, day.Date));
		var vacationsByUser = vacations.ToLookup(vacation => vacation.UserId);
		var events = await GetEventsAsync(today, tomorrow, cancellationToken);

		return new DashboardSummaryDto(
			nextEvent,
			openTaskCount,
			BuildDay(today, members, declaredByUserAndDate, vacationsByUser, events),
			BuildDay(tomorrow, members, declaredByUserAndDate, vacationsByUser, events));
	}

	#endregion

	#region Private Methods

	/// <summary>Loads every event starting between the beginning of <paramref name="from"/> and the end of <paramref name="to"/>.</summary>
	private async Task<List<EventDto>> GetEventsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken)
	{
		var rangeStart = ToUtcDayStart(from);
		var rangeEnd = ToUtcDayStart(to.AddDays(1));

		return await dbContext.Events
			.Where(e => e.StartsAtUtc >= rangeStart && e.StartsAtUtc < rangeEnd)
			.OrderBy(e => e.StartsAtUtc)
			.Select(e => new EventDto(e.Id, e.Title, e.Type.ToString(), e.StartsAtUtc, e.Location, e.Notes))
			.ToListAsync(cancellationToken);
	}

	private static DailyTeamStatusDto BuildDay(
		DateOnly date,
		List<DashboardMember> members,
		IReadOnlyDictionary<(Guid UserId, DateOnly Date), PlayerAvailabilityDay> declaredByUserAndDate,
		ILookup<Guid, Vacation> vacationsByUser,
		List<EventDto> events)
	{
		var statuses = members
			.Select(member =>
			{
				var effective = EffectiveAvailabilityCalculator.Resolve(
					member.Id,
					date,
					declaredByUserAndDate,
					vacationsByUser);

				return new MemberDayStatusDto(
					member.Id,
					member.DisplayName,
					member.TeamRole,
					effective.Status,
					effective.From,
					effective.To,
					effective.IsVacation);
			})
			.ToList();

		var dayEvent = events.FirstOrDefault(e => DateOnly.FromDateTime(e.StartsAtUtc) == date);

		return new DailyTeamStatusDto(date, statuses, dayEvent);
	}

	private static DateTime ToUtcDayStart(DateOnly date) =>
		DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

	#endregion
}
