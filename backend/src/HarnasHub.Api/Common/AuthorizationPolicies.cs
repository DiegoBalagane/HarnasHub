namespace HarnasHub.Api.Common;

/// <summary>Names of the authorization policies registered in <c>Program.cs</c>.</summary>
public static class AuthorizationPolicies
{
	#region Public Fields

	/// <summary>Any account that already has a team access level — everyone except a pending Guest.</summary>
	public const string TeamMember = "TeamMember";

	#endregion
}
