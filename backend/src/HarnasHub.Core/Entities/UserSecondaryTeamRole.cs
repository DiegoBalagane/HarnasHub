using HarnasHub.Core.Enums;

namespace HarnasHub.Core.Entities;

/// <summary>An additional/backup in-game role a player also covers, alongside their primary <see cref="User.TeamRole"/> — e.g. a main Rifler who is also the team's second AWPer or second IGL.</summary>
public class UserSecondaryTeamRole
{
	#region Public Properties

	public Guid Id { get; set; }
	public Guid UserId { get; set; }
	public TeamRole TeamRole { get; set; }

	#endregion
}
