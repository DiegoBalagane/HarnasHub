namespace HarnasHub.Core.Enums;

/// <summary>Access level of an account within HarnasHub; new sign-ups start as <see cref="Guest"/> until a Manager promotes them.</summary>
public enum UserRole
{
	Guest = 0,
	Player = 1,
	Coach = 2,
	Manager = 3
}
