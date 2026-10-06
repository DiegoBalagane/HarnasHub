#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Common.Faceit;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Common.Faceit;

public class FaceitFactionMatcherTests
{
	#region Public Methods

	[Fact]
	public void Should_find_our_faction_by_steam_id_or_cached_faceit_id()
	{
		var factions = Factions();

		Assert.Equal(0, FaceitFactionMatcher.OurFactionIndex(factions, new HashSet<string> { "101" }, new HashSet<string>()));
		Assert.Equal(1, FaceitFactionMatcher.OurFactionIndex(factions, new HashSet<string>(), new HashSet<string> { "b1" }));
		Assert.Null(FaceitFactionMatcher.OurFactionIndex(factions, new HashSet<string> { "101", "201" }, new HashSet<string>()));
		Assert.Null(FaceitFactionMatcher.OurFactionIndex(factions, new HashSet<string>(), new HashSet<string>()));
	}

	[Fact]
	public void Should_prefer_known_opponent_players_then_the_faction_that_is_not_us_then_the_name()
	{
		var factions = Factions();

		Assert.Equal(1, FaceitFactionMatcher.OpponentFactionIndex(factions, 1, new HashSet<string> { "b1", "b2" }, "rivals"));
		Assert.Equal(1, FaceitFactionMatcher.OpponentFactionIndex(factions, 0, new HashSet<string>(), "whoever"));
		Assert.Equal(1, FaceitFactionMatcher.OpponentFactionIndex(factions, null, new HashSet<string>(), "rivals"));
		Assert.Null(FaceitFactionMatcher.OpponentFactionIndex(factions, null, new HashSet<string>(), "nobody"));
	}

	[Fact]
	public void Should_tell_which_demo_team_a_faction_played_as()
	{
		var factions = Factions();

		Assert.Equal("B", FaceitFactionMatcher.DemoTeam(factions[0], [201, 202], [101, 102]));
		Assert.Equal("A", FaceitFactionMatcher.DemoTeam(factions[1], [201, 202], [101, 102]));
		Assert.Null(FaceitFactionMatcher.DemoTeam(factions[0], [999], [998]));
	}

	#endregion

	#region Private Methods

	private static List<FaceitFactionInfo> Factions() =>
	[
		new("f1", "team_Alpha", [new("a1", "Alpha", "101", 10), new("a2", "Bravo", "102", 9)]),
		new("f2", "Rivals Esports", [new("b1", "Charlie", "201", 8), new("b2", "Delta", "202", 8)])
	];

	#endregion
}
