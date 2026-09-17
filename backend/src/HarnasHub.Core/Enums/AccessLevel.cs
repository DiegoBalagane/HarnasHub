namespace HarnasHub.Core.Enums;

/// <summary>Permission level of an account within HarnasHub; new sign-ups start as <see cref="Guest"/> until a Manager promotes them. Being the team's coach is a separate flag (<c>User.IsCoach</c>), not an access level.</summary>
public enum AccessLevel
{
	Guest = 0,
	Player = 1,
	Manager = 2
}
