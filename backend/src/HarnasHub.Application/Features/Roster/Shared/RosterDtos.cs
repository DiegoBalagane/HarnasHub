namespace HarnasHub.Application.Features.Roster.Shared;

/// <summary>A single team member as shown in the roster; <paramref name="Role"/> is the access level ("Guest"/"Player"/"Manager") and <paramref name="IsCoach"/> is the independent coach tag, so a Manager can also be the coach; <paramref name="TeamRole"/> is the in-game role and may be unset; <paramref name="RosterSlot"/> is Main/Bench/StandIn and may be unset; <paramref name="PinColor"/> is only ever set for Main-roster players; <paramref name="PinMark"/> is a self-chosen single character shown on the member's map-radar pin, settable by anyone; <paramref name="SecondaryTeamRoles"/> lists any backup roles alongside the primary one (e.g. "second AWPer").</summary>
public record TeamMemberDto(
	Guid Id,
	string DisplayName,
	string Role,
	bool IsCoach,
	string? AvatarUrl,
	string? TeamRole,
	string? RosterSlot,
	string? PinColor,
	string? PinMark,
	string? InGameNickname,
	string? SteamId64,
	List<string> SecondaryTeamRoles);
