using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.OpponentReport.Shared;

public class OpponentVetoPredictorTests
{
	#region Public Methods

	[Fact]
	public void Should_predict_bans_on_maps_they_never_play_and_picks_on_their_favourites()
	{
		var metrics = Metrics(new()
		{
			[MapName.Mirage] = (8, 6),
			[MapName.Ancient] = (5, 3),
			[MapName.Inferno] = (3, 1),
			[MapName.Nuke] = (1, 0)
		});

		var predictions = OpponentVetoPredictor.Predict(metrics, totalGames: 17);

		Assert.Equal("Pick", predictions[MapName.Mirage].Prediction);
		Assert.Equal("Pick", predictions[MapName.Ancient].Prediction);
		Assert.Equal("Neutral", predictions[MapName.Inferno].Prediction);
		Assert.Equal("Ban", predictions[MapName.Nuke].Prediction);
		Assert.Equal("Ban", predictions[MapName.Dust2].Prediction);
		Assert.Contains("prawdopodobny ban", predictions[MapName.Dust2].Reason);
	}

	[Fact]
	public void Should_not_flag_a_most_played_but_losing_map_as_a_pick()
	{
		var metrics = Metrics(new() { [MapName.Mirage] = (10, 1), [MapName.Ancient] = (4, 3) });

		var predictions = OpponentVetoPredictor.Predict(metrics, totalGames: 14);

		Assert.NotEqual("Pick", predictions[MapName.Mirage].Prediction);
		Assert.Equal("Pick", predictions[MapName.Ancient].Prediction);
	}

	[Fact]
	public void Should_mark_everything_unknown_with_too_few_games_but_still_rank_preference()
	{
		var metrics = Metrics(new() { [MapName.Mirage] = (2, 2) });

		var predictions = OpponentVetoPredictor.Predict(metrics, totalGames: 2);

		Assert.All(predictions.Values, p => Assert.Equal("Unknown", p.Prediction));
		Assert.True(predictions[MapName.Mirage].Preference > predictions[MapName.Nuke].Preference);
		Assert.Equal(0, predictions[MapName.Nuke].Preference);
	}

	[Fact]
	public void Should_keep_team_based_picks_and_upgrade_a_barely_played_map_avoided_solo_to_a_ban()
	{
		var metrics = Metrics(new()
		{
			[MapName.Mirage] = (8, 6),
			[MapName.Ancient] = (5, 3),
			[MapName.Inferno] = (3, 1),
			[MapName.Anubis] = (2, 1),
			[MapName.Nuke] = (1, 0)
		});
		var comfort = new Dictionary<MapName, MapComfort>
		{
			[MapName.Mirage] = IndividualSignalTests.Comfort(5, 0, 5, map: MapName.Mirage),
			[MapName.Inferno] = IndividualSignalTests.Comfort(5, 0, 4, map: MapName.Inferno),
			[MapName.Anubis] = IndividualSignalTests.Comfort(5, 0, 4, map: MapName.Anubis)
		};

		var withoutSolo = OpponentVetoPredictor.Predict(metrics, totalGames: 19);
		var withSolo = OpponentVetoPredictor.Predict(metrics, totalGames: 19, comfort);

		Assert.Equal("Neutral", withoutSolo[MapName.Anubis].Prediction);
		Assert.Equal("Ban", withSolo[MapName.Anubis].Prediction);
		Assert.Contains("unika jej także solo", withSolo[MapName.Anubis].Reason);
		Assert.Equal("Neutral", withSolo[MapName.Inferno].Prediction);
		Assert.Equal("Pick", withSolo[MapName.Mirage].Prediction);
		Assert.Equal("Pick", withSolo[MapName.Ancient].Prediction);
		Assert.True(withSolo[MapName.Anubis].Preference < withoutSolo[MapName.Anubis].Preference);
		Assert.Equal(withoutSolo[MapName.Ancient].Preference, withSolo[MapName.Ancient].Preference);
	}

	[Fact]
	public void Should_call_bans_and_picks_from_solo_comfort_when_team_games_are_scarce()
	{
		var metrics = Metrics(new() { [MapName.Mirage] = (2, 1) });
		var comfort = new Dictionary<MapName, MapComfort>
		{
			[MapName.Ancient] = IndividualSignalTests.Comfort(5, 0, 4, map: MapName.Ancient),
			[MapName.Inferno] = IndividualSignalTests.Comfort(5, 4, 0, share: 0.3, smoothed: 0.6, map: MapName.Inferno)
		};

		var predictions = OpponentVetoPredictor.Predict(metrics, totalGames: 2, comfort);

		Assert.Equal("Ban", predictions[MapName.Ancient].Prediction);
		Assert.Equal("Pick", predictions[MapName.Inferno].Prediction);
		Assert.Contains("wg meczów solo", predictions[MapName.Inferno].Reason);
		Assert.Equal("Unknown", predictions[MapName.Mirage].Prediction);
		Assert.Equal("Unknown", predictions[MapName.Nuke].Prediction);
		Assert.True(predictions[MapName.Inferno].Preference > predictions[MapName.Nuke].Preference);
		Assert.Equal(0, predictions[MapName.Ancient].Preference);
	}

	#endregion

	#region Private Methods

	private static Dictionary<MapName, MapMetrics> Metrics(Dictionary<MapName, (int Games, int Wins)> played)
	{
		var total = played.Values.Sum(p => p.Games);
		return Enum.GetValues<MapName>().ToDictionary(map => map, map =>
		{
			var (games, wins) = played.GetValueOrDefault(map);
			return new MapMetrics(
				map,
				games,
				wins,
				games == 0 ? null : (double)wins / games,
				MapAdvantage.SmoothedWinRate(wins, games),
				null,
				null,
				null,
				null,
				total == 0 ? 0 : (double)games / total);
		});
	}

	#endregion
}
