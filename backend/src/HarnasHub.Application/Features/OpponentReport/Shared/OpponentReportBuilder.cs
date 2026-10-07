using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Veto.Shared;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>Everything the report is computed from, already loaded: cached FACEIT maps in the window, the opponent's scoreboard
/// lines, both rosters and the internal veto inputs (map pool, our results, head-to-head, tactics); optionally our players'
/// scoreboard lines and cached profiles for their individual form, and both sides' cached lifetime per-map stats by player id.</summary>
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
	IReadOnlyList<FaceitPlayerDto>? OurPlayers = null,
	IReadOnlyDictionary<string, IReadOnlyList<FaceitLifetimeMapStats>>? TheirLifetime = null,
	IReadOnlyDictionary<string, IReadOnlyList<FaceitLifetimeMapStats>>? OurLifetime = null);

/// <summary>Pure assembly of <see cref="OpponentReportDto"/> from <see cref="OpponentReportInput"/> — no I/O, fully unit-testable.
/// Team games and the lineup come from <see cref="OpponentTeamGames"/> (official games of the linked FACEIT team + games of ≥ 3 of
/// the lineup together); everything player-facing uses only the lineup.</summary>
public static class OpponentReportBuilder
{
	#region Public Methods

	/// <summary>Builds the full report; live-only fields (FACEIT configured, next event) are left for the caller to fill.</summary>
	public static OpponentReportDto Build(OpponentReportInput input)
	{
		var now = input.GeneratedAtUtc;
		var nicknames = OpponentReportRows.Nicknames(input);
		var (theirGames, lineup) = OpponentTeamGames.Select(
			input.Matches, input.Link?.FaceitTeamId, input.TheirRoster, now, nicknames, input.Link?.Players ?? []);
		var ourGames = TeamMatchDetector.Detect(input.Matches, input.OurRoster);
		var active = lineup.ActiveIds;
		var inactive = input.TheirRoster.Where(id => !active.Contains(id)).ToHashSet();

		var theirs = MapMetricsCalculator.Calculate(theirGames, now);
		var ours = MapMetricsCalculator.Calculate(ourGames);
		var individual = IndividualFormBuilder.Build(input, active, theirGames);
		var theirLifetime = Lifetime(input.TheirLifetime, active);
		var ourLifetime = Lifetime(input.OurLifetime, input.OurRoster);
		var predictions = OpponentVetoPredictor.Predict(theirs, theirGames.Count, individual.TheirComfort, theirLifetime);
		var vetoInputs = input.VetoInputs.ToDictionary(i => i.MapName);

		var maps = Enum.GetValues<MapName>().Select(map =>
		{
			var their = theirs[map];
			var vetoInput = vetoInputs.GetValueOrDefault(map) ?? new MapVetoInput(map, null, 0, 0, 0, 0, 0, 0, 0, 0);
			var internalGames = vetoInput.Wins + vetoInput.Losses + vetoInput.Draws;
			var ourWins = ours[map].Wins + vetoInput.Wins + 0.5 * vetoInput.Draws;
			var ourTotal = ours[map].Games + internalGames;
			var ourPrior = IndividualSignal.WinRatePrior(individual.OurComfort.GetValueOrDefault(map));
			// Solo form only shifts each side's smoothing prior (capped 0.4–0.6, weight k/(k+games)) — see IndividualSignal.
			var advantage = MapAdvantage.Advantage(
				ourWins,
				ourTotal,
				their.WeightedWins ?? their.Wins,
				their.WeightedGames ?? their.Games,
				ourPrior,
				IndividualSignal.WinRatePrior(individual.TheirComfort.GetValueOrDefault(map)));

			return new OpponentReportRows.MapContext(
				map,
				their,
				ours[map].Games,
				ours[map].Wins,
				internalGames,
				ourWins,
				ourTotal,
				advantage,
				MapAdvantage.Confidence(Math.Min(ourTotal, their.Games)),
				OpponentReportSnapshots.WithFaceit(
					vetoInput,
					their.Games,
					their.Wins,
					their.SmoothedWinRate,
					ours[map].Games,
					ours[map].Wins,
					ourPrior == 0.5 ? null : ourPrior),
				theirLifetime[map],
				ourLifetime[map]);
		}).ToList();

		var suggestions = VetoScoring.Suggest(maps.Select(m => m.VetoInput)).ToDictionary(s => s.MapName);
		var rows = maps
			.Select(m => OpponentReportRows.ToRow(m, suggestions[m.Map.ToString()], predictions[m.Map]) with
			{
				TheirOfficialGames = theirGames.Count(g => g.Map == m.Map && g.Official),
				TheirTogetherGames = theirGames.Count(g => g.Map == m.Map && !g.Official)
			})
			.OrderByDescending(r => r.TheirGames)
			.ThenByDescending(r => r.OurGames)
			.ThenBy(r => r.MapName)
			.ToList();

		var candidates = maps
			.Select(m => new VetoCandidate(
				m.Map,
				suggestions[m.Map.ToString()].Score,
				predictions[m.Map].Preference,
				suggestions[m.Map.ToString()].Note ?? "",
				VetoNotes.Theirs(m.Their)))
			.ToList();
		var plans = new List<VetoPlanDto>
		{
			new(nameof(VetoFormat.Bo1), VetoSimulator.Simulate(candidates, VetoFormat.Bo1)),
			new(nameof(VetoFormat.Bo3), VetoSimulator.Simulate(candidates, VetoFormat.Bo3))
		};

		var playersToWatch = PlayersToWatchCalculator.Calculate(OpponentReportRows.PlayerLines(input, active));
		var form = TeamFormCalculator.Calculate(theirGames, nicknames, inactive);
		var insights = OpponentInsightRules.Build(new InsightInput(theirGames.Count, rows, playersToWatch, form, individual.Form));

		return new OpponentReportDto(
			input.OpponentName,
			false,
			input.Link,
			input.GeneratedAtUtc,
			input.Link?.LastSyncedAtUtc,
			theirGames.Count,
			SoloGames(input.Matches, theirGames, active),
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
			IndividualForm = individual.Form,
			ActiveLineup = lineup.Dto
		};
	}

	#endregion

	#region Private Methods

	/// <summary>Games with a lineup player that aren't team games — the solo sample behind their player comfort.</summary>
	private static int SoloGames(IEnumerable<FaceitMatch> matches, IEnumerable<TeamGame> teamGames, IReadOnlySet<string> lineup)
	{
		var teamRows = teamGames.Select(g => g.RowId).ToHashSet();
		return matches.Count(m => !teamRows.Contains(m.Id) && m.Team1PlayerIds.Concat(m.Team2PlayerIds).Any(lineup.Contains));
	}

	/// <summary>Per-map lifetime aggregate over the given players' cached stats.</summary>
	private static Dictionary<MapName, MapLifetime> Lifetime(
		IReadOnlyDictionary<string, IReadOnlyList<FaceitLifetimeMapStats>>? stats,
		IReadOnlySet<string> players) =>
		LifetimeMapCalculator.Calculate(players
			.Select(id => stats?.GetValueOrDefault(id))
			.Where(s => s is not null)
			.Select(s => s!));

	#endregion
}
