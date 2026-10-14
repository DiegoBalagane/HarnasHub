using HarnasHub.Api.Endpoints.Auth;
using Xunit;

namespace HarnasHub.Tests.Api.Endpoints;

/// <summary>Tests for the OAuth state comparison guarding the Discord callback against login CSRF.</summary>
public class AuthEndpointsStateTests
{
	#region Public Methods

	[Fact]
	public void IsStateValid_MatchingValues_ReturnsTrue()
	{
		Assert.True(AuthEndpoints.IsStateValid("abc123", "abc123"));
	}

	[Theory]
	[InlineData("abc123", "abc124")]
	[InlineData("abc123", null)]
	[InlineData(null, "abc123")]
	[InlineData("", "")]
	[InlineData(null, null)]
	public void IsStateValid_MismatchedOrMissing_ReturnsFalse(string? expected, string? actual)
	{
		Assert.False(AuthEndpoints.IsStateValid(expected, actual));
	}

	#endregion
}
