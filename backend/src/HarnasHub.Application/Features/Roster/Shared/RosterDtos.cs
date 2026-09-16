namespace HarnasHub.Application.Features.Roster.Shared;

/// <summary>A single team member as shown in the roster; <paramref name="TeamRole"/> is the in-game role and may be unset.</summary>
public record TeamMemberDto(
	Guid Id,
	string DisplayName,
	string Role,
	string? AvatarUrl,
	string? TeamRole,
	string? InGameNickname);
