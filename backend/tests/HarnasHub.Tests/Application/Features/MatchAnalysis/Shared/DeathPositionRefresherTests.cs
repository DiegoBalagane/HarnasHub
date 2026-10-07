#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Application.Features.Stats.Shared;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using System.Text.Json;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.MatchAnalysis.Shared;

public class DeathPositionRefresherTests
{
	#region Public Methods

	[Fact]
	public void Should_match_by_steam_id_before_name_and_touch_only_death_positions()
	{
		var userId = Guid.NewGuid();
		var row = new PlayerMatchStat { UserId = userId, DemoPlayerName = "Other", Kills = 20, Adr = 80.5, DeathPositionsJson = "[]" };
		var players = new[] { Player(1, "Other", (0.9f, 0.9f)), Player(7, "Me", (0.1f, 0.2f)) };

		var refreshed = DeathPositionRefresher.Refresh([row], new Dictionary<Guid, long> { [userId] = 7 }, players);

		Assert.Equal(1, refreshed);
		var dots = JsonSerializer.Deserialize<List<DeathPositionDto>>(row.DeathPositionsJson!)!;
		Assert.Equal(new DeathPositionDto(0.1f, 0.2f, "T"), dots.Single());
		Assert.Equal(20, row.Kills);
		Assert.Equal(80.5, row.Adr);
	}

	[Fact]
	public void Should_fall_back_to_case_insensitive_name_and_leave_unmatched_rows_alone()
	{
		var byName = new PlayerMatchStat { DemoPlayerName = "ZYWIEC" };
		var unmatched = new PlayerMatchStat { DemoPlayerName = "ghost", DeathPositionsJson = "keep" };

		var refreshed = DeathPositionRefresher.Refresh([byName, unmatched], new Dictionary<Guid, long>(), [Player(5, "zywiec", (0.5f, 0.5f))]);

		Assert.Equal(1, refreshed);
		Assert.NotNull(byName.DeathPositionsJson);
		Assert.Equal("keep", unmatched.DeathPositionsJson);
	}

	[Fact]
	public void Should_clear_positions_when_the_player_has_no_deaths()
	{
		var row = new PlayerMatchStat { DemoPlayerName = "a", DeathPositionsJson = "[{}]" };

		DeathPositionRefresher.Refresh([row], new Dictionary<Guid, long>(), [Player(1, "a")]);

		Assert.Null(row.DeathPositionsJson);
	}

	#endregion

	#region Private Methods

	/// <summary>A parsed demo player with T-side deaths at the given radar fractions.</summary>
	public static DemoPlayerStats Player(long steamId, string name, params (float X, float Y)[] deaths) =>
		new(steamId, name, 0, deaths.Length, 0, 0, 0, 0, 0, 0, 0, 0, new Dictionary<int, int>(),
			deaths.Select(d => new DemoDeathPosition(d.X, d.Y, MapSide.T)).ToList());

	#endregion
}
