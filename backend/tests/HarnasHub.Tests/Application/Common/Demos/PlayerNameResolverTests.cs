using HarnasHub.Application.Common.Demos;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Common.Demos;

public class PlayerNameResolverTests
{
	#region Public Methods

	[Theory]
	[InlineData(null, true)]
	[InlineData("", true)]
	[InlineData("76561198393204503", true)]
	[InlineData("kenny", false)]
	[InlineData("123abc", false)]
	public void IsUnnamed_detects_missing_and_numeric_names(string? name, bool expected) =>
		Assert.Equal(expected, PlayerNameResolver.IsUnnamed(name));

	[Fact]
	public void Fallback_uses_the_last_four_digits() =>
		Assert.Equal("Gracz …4503", PlayerNameResolver.Fallback(76561198393204503));

	[Fact]
	public async Task ResolveAsync_prefers_demo_then_roster_then_faceit_then_fallback()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.Users.Add(new User
		{
			Id = Guid.NewGuid(),
			DiscordId = "1",
			DisplayName = "Kacper",
			InGameNickname = "harnas",
			AccessLevel = AccessLevel.Player,
			SteamId64 = "76561198000000002",
			CreatedAtUtc = DateTime.UtcNow
		});
		dbContext.FaceitPlayers.Add(new FaceitPlayer { Id = "f1", Nickname = "faceitGuy", SteamId64 = "76561198000000003" });
		dbContext.FaceitPlayers.Add(new FaceitPlayer { Id = "f2", Nickname = "shadowed", SteamId64 = "76561198000000002" });
		await dbContext.SaveChangesAsync();

		var names = await PlayerNameResolver.ResolveAsync(dbContext, new Dictionary<long, string?>
		{
			[76561198000000001] = "demoName",
			[76561198000000002] = "76561198000000002",
			[76561198000000003] = null,
			[76561198000000004] = ""
		}, CancellationToken.None);

		Assert.Equal("demoName", names[76561198000000001]);
		Assert.Equal("harnas", names[76561198000000002]);
		Assert.Equal("faceitGuy", names[76561198000000003]);
		Assert.Equal("Gracz …0004", names[76561198000000004]);
	}

	#endregion
}
