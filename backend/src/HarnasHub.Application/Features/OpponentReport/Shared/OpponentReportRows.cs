using HarnasHub.Application.Features.Veto.Shared;
using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>Row-level helpers of <see cref="OpponentReportBuilder"/>: the per-map context, matrix rows, player lines and nicknames.</summary>
public static class OpponentReportRows
{
	#region Public Types

	/// <summary>Per-map intermediate values shared by the matrix row, the veto score and the simulation notes.</summary>
	public sealed record MapContext(
		MapName Map,
		MapMetrics Their,
		int OurFaceitGames,
		int OurFaceitWins,
		int InternalGames,
		double OurWins,
		int OurTotal,
		double Advantage,
		ConfidenceLevel Confidence,
		MapVetoInput VetoInput,
		MapLifetime TheirLifetime,
		MapLifetime OurLifetime);

	#endregion

	#region Public Methods

	/// <summary>One matrix row, fractions turned into percentages.</summary>
	public static MapComparisonDto ToRow(MapContext m, MapVetoSuggestionDto suggestion, VetoPrediction prediction) =>
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
			suggestion.Reasons)
		{
			OurSmoothedWinRate = m.OurTotal == 0 ? null : Percent(MapAdvantage.SmoothedWinRate(m.OurWins, m.OurTotal)),
			TheirSmoothedWinRate = m.Their.Games == 0 ? null : Percent(m.Their.SmoothedWinRate),
			OurFaceitWins = m.OurFaceitWins,
			OurSoloPrior = m.VetoInput.OurSoloPrior,
			OurPlaysIndividually = m.VetoInput.OurPlaysIndividually,
			OurLowSample = !SampleThresholds.HasWinRateSample(m.OurTotal),
			TheirLowSample = !SampleThresholds.HasWinRateSample(m.Their.Games),
			TheirLifetime = LifetimeMapCalculator.ToDto(m.TheirLifetime),
			OurLifetime = LifetimeMapCalculator.ToDto(m.OurLifetime)
		};

	/// <summary>Scoreboard lines of the active lineup (<paramref name="players"/>) on pool maps, oldest first so the latest nickname
	/// ends up last.</summary>
	public static IEnumerable<PlayerGameLine> PlayerLines(OpponentReportInput input, IReadOnlySet<string> players)
	{
		var maps = input.Matches.ToDictionary(m => m.Id);
		return input.TheirStats
			.Where(s => players.Contains(s.PlayerId) && maps.ContainsKey(s.MatchId))
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
	public static Dictionary<string, string> Nicknames(OpponentReportInput input)
	{
		var nicknames = input.Link?.Players.ToDictionary(p => p.PlayerId, p => p.Nickname) ?? new Dictionary<string, string>();
		var playedAt = input.Matches.ToDictionary(m => m.Id, m => m.PlayedAtUtc);
		foreach (var stat in input.TheirStats.OrderBy(s => playedAt.GetValueOrDefault(s.MatchId)))
		{
			nicknames[stat.PlayerId] = stat.Nickname;
		}

		return nicknames;
	}

	/// <summary>A fraction as a percentage with one decimal; null stays null.</summary>
	public static double? Percent(double? fraction) => fraction is { } value ? Math.Round(value * 100, 1) : null;

	#endregion
}
