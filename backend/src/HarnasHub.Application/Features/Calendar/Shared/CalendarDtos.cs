namespace HarnasHub.Application.Features.Calendar.Shared;

/// <summary>A calendar event as shown in lists.</summary>
public record EventDto(
	Guid Id,
	string Title,
	string Type,
	DateTime StartsAtUtc,
	string? Location,
	string? Notes);

/// <summary>One team member's availability for an event, or "NotSet" if they haven't declared one.</summary>
public record MemberAvailabilityDto(Guid UserId, string DisplayName, string Status);
