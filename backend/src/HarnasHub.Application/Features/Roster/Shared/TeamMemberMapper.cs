using HarnasHub.Core.Entities;

namespace HarnasHub.Application.Features.Roster.Shared;

/// <summary>Maps a loaded <see cref="User"/> onto the roster DTO returned by the write-side slices.</summary>
public static class TeamMemberMapper
{
	#region Public Methods

	/// <summary>Projects an already-materialised user entity to <see cref="TeamMemberDto"/>. Callers that haven't loaded secondary roles get an empty list back.</summary>
	public static TeamMemberDto ToTeamMemberDto(this User user, IReadOnlyList<string>? secondaryTeamRoles = null) => new(
		user.Id,
		user.DisplayName,
		user.Role.ToString(),
		user.AvatarUrl,
		user.TeamRole?.ToString(),
		user.RosterSlot?.ToString(),
		user.PinColor?.ToString(),
		user.InGameNickname,
		secondaryTeamRoles?.ToList() ?? []);

	#endregion
}
