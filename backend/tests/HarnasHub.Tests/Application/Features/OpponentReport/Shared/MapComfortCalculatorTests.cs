using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Core.Enums;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.IndividualLineFactory;

namespace HarnasHub.Tests.Application.Features.OpponentReport.Shared;

public class MapComfortCalculatorTests
{
	#region Public Methods

	[Fact]
	public void Should_count_regulars_and_avoiders_from_solo_games_of_rated_players_only()
	{
		var lines = new List<IndividualGameLine>();
		foreach (var id in new[] { "p1", "p2", "p3", "p4" })
		{
			lines.AddRange(Lines(id, MapName.Mirage, 8, wins: 6, kills: 26, deaths: 20));
			lines.AddRange(Lines(id, MapName.Inferno, 4, wins: 2, daysAgo: 20));
		}

		lines.AddRange(Lines("p5", MapName.Ancient, 12, wins: 6));
		// Not rated (too few solo games), and team games never count towards comfort.
		lines.AddRange(Lines("p6", MapName.Ancient, 5));
		lines.AddRange(Lines("p1", MapName.Ancient, 9, team: true, daysAgo: 40));

		var comfort = MapComfortCalculator.Calculate(lines);

		var mirage = comfort[MapName.Mirage];
		Assert.Equal((5, 4, 1), (mirage.RatedPlayers, mirage.RegularPlayers, mirage.AvoidingPlayers));
		Assert.Equal(0.75, mirage.AvgWinRate);
		Assert.Equal(1.3, mirage.AvgKdRatio!.Value, 3);
		Assert.True(mirage.IsComfortable);
		var ancient = comfort[MapName.Ancient];
		Assert.Equal((1, 4), (ancient.RegularPlayers, ancient.AvoidingPlayers));
		Assert.True(ancient.IsAvoided);
		Assert.False(ancient.IsComfortable);
		Assert.Equal(["nick-p1", "nick-p2", "nick-p3", "nick-p4"], ancient.AvoidingNicknames);
		Assert.Equal(0.2, ancient.SoloShare, 3);

		var dto = MapComfortCalculator.ToDtos(comfort).First();
		Assert.Equal(("Mirage", 75.0), (dto.MapName, dto.AvgWinRate!.Value));
	}

	[Fact]
	public void ToDtos_should_be_empty_when_nobody_is_rated()
	{
		var comfort = MapComfortCalculator.Calculate(Lines("p1", MapName.Mirage, MapComfortCalculator.MinSoloGamesToRate - 1));

		Assert.Equal(0, comfort[MapName.Mirage].RatedPlayers);
		Assert.Empty(MapComfortCalculator.ToDtos(comfort));
	}

	#endregion
}
