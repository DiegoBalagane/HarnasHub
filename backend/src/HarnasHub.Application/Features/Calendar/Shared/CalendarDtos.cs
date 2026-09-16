namespace HarnasHub.Application.Features.Calendar.Shared;

/// <summary>A calendar event as shown in lists.</summary>
public record EventDto(
	Guid Id,
	string Title,
	string Type,
	DateTime StartsAtUtc,
	string? Location,
	string? Notes);

/// <summary>One team member's availability for an event, or "NotSet" if they haven't declared one; <paramref name="InGameNickname"/> is their chosen display nickname, null when they haven't set one (fall back to <paramref name="DisplayName"/>, the Discord name).</summary>
public record MemberAvailabilityDto(Guid UserId, string DisplayName, string? InGameNickname, string Status);
