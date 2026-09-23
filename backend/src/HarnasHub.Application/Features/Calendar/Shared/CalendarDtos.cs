namespace HarnasHub.Application.Features.Calendar.Shared;

/// <summary>A calendar event as shown in lists; <paramref name="Url"/> is an optional link to the match/stream/lobby, shown as a clickable link rather than jammed into <paramref name="Location"/>.</summary>
public record EventDto(
	Guid Id,
	string Title,
	string Type,
	DateTime StartsAtUtc,
	DateTime? EndsAtUtc,
	string? Location,
	string? Url,
	string? Notes);

/// <summary>One team member's availability for an event, or "NotSet" if they haven't declared one; <paramref name="InGameNickname"/> is their chosen display nickname, null when they haven't set one (fall back to <paramref name="DisplayName"/>, the Discord name).</summary>
public record MemberAvailabilityDto(Guid UserId, string DisplayName, string? InGameNickname, string Status);
