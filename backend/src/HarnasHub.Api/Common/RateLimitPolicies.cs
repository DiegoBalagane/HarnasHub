namespace HarnasHub.Api.Common;

/// <summary>Names of the rate-limiting policies registered in <c>Program.cs</c>.</summary>
public static class RateLimitPolicies
{
	#region Public Fields

	/// <summary>Strict per-IP policy for sign-in and session-refresh endpoints.</summary>
	public const string Auth = "auth";

	#endregion
}
