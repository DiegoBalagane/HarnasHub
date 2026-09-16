namespace HarnasHub.Application.Features.Roster.Shared;

/// <summary>A single team member as shown in the roster; <paramref name="TeamRole"/> is the in-game role and may be unset; <paramref name="RosterSlot"/> is Main/Bench/StandIn and may be unset; <paramref name="PinColor"/> is only ever set for Main-roster players; <paramref name="SecondaryTeamRoles"/> lists any backup roles alongside the primary one (e.g. "second AWPer").</summary>
public record TeamMemberDto(
	Guid Id,
	string DisplayName,
	string Role,
	string? AvatarUrl,
	string? TeamRole,
	string? RosterSlot,
	string? PinColor,
	string? InGameNickname,
	List<string> SecondaryTeamRoles);
