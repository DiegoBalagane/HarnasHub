namespace HarnasHub.Application.Features.Availability.Shared;

/// <summary>Weekly availability grid — one row per team member.</summary>
public record WeekAvailabilityDto(List<MemberWeekDto> Members);

/// <summary>One team member's row in the weekly grid; <paramref name="TeamRole"/> is the member's in-game role, null when unassigned.</summary>
public record MemberWeekDto(Guid UserId, string DisplayName, string? TeamRole, List<DayEntryDto> Days);

/// <summary>Effective availability for one member on one day ("NotSet" when nothing was declared).</summary>
public record DayEntryDto(DateOnly Date, string Status, TimeOnly? From, TimeOnly? To, bool IsVacation, string? Note);

/// <summary>A single time-off range belonging to the current user.</summary>
public record VacationDto(Guid Id, DateOnly StartDate, DateOnly EndDate, string? Reason);
