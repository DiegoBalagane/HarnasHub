using HarnasHub.Application.Features.Calendar.Shared;

namespace HarnasHub.Application.Features.Dashboard.Shared;

/// <summary>Summary shown on the team dashboard for the current user.</summary>
public record DashboardSummaryDto(
	EventDto? NextEvent,
	int OpenTaskCount,
	DailyTeamStatusDto Today,
	DailyTeamStatusDto Tomorrow);

/// <summary>Who is available on a single day, plus that day's earliest event if there is one.</summary>
public record DailyTeamStatusDto(DateOnly Date, List<MemberDayStatusDto> Members, EventDto? Event);

/// <summary>One member's effective status for a single day ("NotSet" when nothing was declared); <paramref name="TeamRole"/> is the member's in-game role, null when unassigned; <paramref name="InGameNickname"/> is their chosen display nickname, null when they haven't set one (fall back to <paramref name="DisplayName"/>, the Discord name).</summary>
public record MemberDayStatusDto(
	Guid UserId,
	string DisplayName,
	string? InGameNickname,
	string? TeamRole,
	string Status,
	TimeOnly? From,
	TimeOnly? To,
	bool IsVacation);
