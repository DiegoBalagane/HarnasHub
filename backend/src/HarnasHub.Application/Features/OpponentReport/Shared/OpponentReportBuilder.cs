using HarnasHub.Application.Features.Veto.Shared;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>Everything the report is computed from, already loaded: cached FACEIT maps in the window, the opponent's scoreboard
/// lines, both rosters and the internal veto inputs (map pool, our results, head-to-head, tactics); optionally our players'
/// scoreboard lines and cached profiles for their individual form.</summary>
public record OpponentReportInput(
	string OpponentName,
	OpponentFaceitLinkDto? Link,
	DateTime GeneratedAtUtc,
	IReadOnlyList<FaceitMatch> Matches,
	IReadOnlyList<FaceitMatchPlayerStat> TheirStats,
	IReadOnlySet<string> TheirRoster,
	IReadOnlySet<string> OurRoster,
	IReadOnlyList<MapVetoInput> VetoInputs,
	IReadOnlyList<FaceitMatchPlayerStat>? OurStats = null,
	IReadOnlyList<FaceitPlayerDto>? OurPlayers = null);

/// <summary>Pure assembly of <see cref="OpponentReportDto"/> from <see cref="OpponentReportInput"/> — no I/O, fully unit-testable.</summary>
public static class OpponentReportBuilder
{
	#region Public Methods

	/// <summary>Builds the full report; live-only fields (FACEIT configured, next event) are left for the caller to fill.</summary>
	public static OpponentReportDto Build(OpponentReportInput input)
	{
		var theirGames = TeamMatchDetector.Detect(input.Matches, input.TheirRoster);
		var ourGames = TeamMatchDetector.Detect(input.Matches, input.OurRoster);
		var theirs = MapMetricsCalculator.Calculate(theirGames);
		var ours = MapMetricsCalculator.Calculate(ourGames);
		var individual = IndividualFormBuilder.Build(input);
		var predictions = OpponentVetoPredictor.Predict(theirs, theirGames.Count, individual.TheirComfort);
		var vetoInputs = input.VetoInputs.ToDictionary(i => i.MapName);

		var maps = Enum.GetValues<MapName>().Select(map =>
		{
			var their = theirs[map];
			var vetoInput = vetoInputs.GetValueOrDefault(map) ?? new MapVetoInput(map, null, 0, 0, 0, 0, 0, 0, 0, 0);
			var internalGames = vetoInput.Wins + vetoInput.Losses + vetoInput.Draws;
			var ourWins = ours[map].Wins + vetoInput.Wins + 0.5 * vetoInput.Draws;
			var ourTotal = ours[map].Games + internalGames;
			// Solo form only shifts each side's smoothing prior (capped 0.4–0.6, weight k/(k+games)) — see IndividualSignal.
			var advantage = MapAdvantage.Advantage(
				ourWins,
				ourTotal,
				their.Wins,
				their.Games,
				IndividualSignal.WinRatePrior(individual.OurComfort.GetValueOrDefault(map)),
				IndividualSignal.WinRatePrior(individual.TheirComfort.GetValueOrDefault(map)));

			return new MapContext(
				map,
				their,
				ours[map].Games,
				internalGames,
				ourWins,
				ourTotal,
				advantage,
				MapAdvantage.Confidence(Math.Min(ourTotal, their.Games)),
				OpponentReportSnapshots.WithFaceit(vetoInput, their.Games, ourTotal, advantage));
		}).ToList();

		var suggestions = VetoScoring.Suggest(maps.Select(m => m.VetoInput)).ToDictionary(s => s.MapName);
		var rows = maps
			.Select(m => ToRow(m, suggestions[m.Map.ToString()], predictions[m.Map]))
			.OrderByDescending(r => r.TheirGames)
			.ThenByDescending(r => r.OurGames)
			.ThenBy(r => r.MapName)
			.ToList();

		var candidates = maps
			.Select(m => new VetoCandidate(
				m.Map,
				suggestions[m.Map.ToString()].Score,
				predictions[m.Map].Preference,
				VetoNotes.Ours(m.VetoInput.Status, m.OurWins, m.OurTotal),
				VetoNotes.Theirs(m.Their)))
			.ToList();
		var plans = new List<VetoPlanDto>
		{
			new(nameof(VetoFormat.Bo1), VetoSimulator.Simulate(candidates, VetoFormat.Bo1)),
			new(nameof(VetoFormat.Bo3), VetoSimulator.Simulate(candidates, VetoFormat.Bo3))
		};

		var nicknames = Nicknames(input);
		var playersToWatch = PlayersToWatchCalculator.Calculate(PlayerLines(input));
		var form = TeamFormCalculator.Calculate(theirGames, nicknames);
		var insights = OpponentInsightRules.Build(new InsightInput(theirGames.Count, rows, playersToWatch, form, individual.Form));

		return new OpponentReportDto(
			input.OpponentName,
			false,
			input.Link,
			input.GeneratedAtUtc,
			input.Link?.LastSyncedAtUtc,
			theirGames.Count,
			TeamMatchDetector.CountSoloGames(input.Matches, input.TheirRoster),
			ourGames.Count,
			maps.Sum(m => m.InternalGames),
			input.OurRoster.Count,
			insights,
			rows,
			plans,
			playersToWatch,
			form,
			null,
			null)
		{
			IndividualForm = individual.Form
		};
	}

	#endregion

	#region Private Methods

	/// <summary>Per-map intermediate values shared by the matrix row, the veto score and the simulation notes.</summary>
	private sealed record MapContext(
		MapName Map,
		MapMetrics Their,
		int OurFaceitGames,
		int InternalGames,
		double OurWins,
		int OurTotal,
		double Advantage,
		ConfidenceLevel Confidence,
		MapVetoInput VetoInput);

	/// <summary>One matrix row, fractions turned into percentages.</summary>
	private static MapComparisonDto ToRow(MapContext m, MapVetoSuggestionDto suggestion, VetoPrediction prediction) =>
		new(
			m.Map.ToString(),
			m.Their.Games,
			m.Their.Wins,
			Percent(m.Their.WinRate),
			m.Their.AvgRoundDiff is { } diff ? Math.Round(diff, 1) : null,
			Percent(m.Their.Share)!.Value,
			m.Their.LastPlayedAtUtc,
			Percent(m.Their.Trend),
			m.OurTotal,
			m.OurWins,
			m.OurTotal == 0 ? null : Percent(m.OurWins / m.OurTotal),
			m.OurFaceitGames,
			m.InternalGames,
			m.VetoInput.Status?.ToString(),
			Percent(m.Advantage)!.Value,
			m.Confidence.ToString(),
			prediction.Prediction,
			prediction.Reason,
			suggestion.Score,
			suggestion.Recommendation,
			suggestion.Reasons);

	/// <summary>Opponent scoreboard lines on pool maps, oldest first so the latest nickname ends up last.</summary>
	private static IEnumerable<PlayerGameLine> PlayerLines(OpponentReportInput input)
	{
		var maps = input.Matches.ToDictionary(m => m.Id);
		return input.TheirStats
			.Where(s => maps.ContainsKey(s.MatchId))
			.Select(s => (Stat: s, Match: maps[s.MatchId], Map: TeamMatchDetector.ParseMap(maps[s.MatchId].MapName)))
			.Where(x => x.Map.HasValue)
			.OrderBy(x => x.Match.PlayedAtUtc)
			.Select(x => new PlayerGameLine(
				x.Stat.PlayerId,
				x.Stat.Nickname,
				x.Map!.Value,
				x.Stat.Kills,
				x.Stat.Deaths,
				x.Stat.Adr,
				x.Stat.HeadshotPercent,
				x.Stat.TripleKills + x.Stat.QuadroKills + x.Stat.PentaKills));
	}

	/// <summary>Player id → nickname from the linked roster, overridden by the latest scoreboard spelling.</summary>
	private static Dictionary<string, string> Nicknames(OpponentReportInput input)
	{
		var nicknames = input.Link?.Players.ToDictionary(p => p.PlayerId, p => p.Nickname) ?? new Dictionary<string, string>();
		foreach (var stat in input.TheirStats)
		{
			nicknames[stat.PlayerId] = stat.Nickname;
		}

		return nicknames;
	}

	/// <summary>A fraction as a percentage with one decimal; null stays null.</summary>
	private static double? Percent(double? fraction) => fraction is { } value ? Math.Round(value * 100, 1) : null;

	#endregion
}
