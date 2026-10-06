namespace HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;

/// <summary>Pure flash statistics of our players from the blind events: enemies blinded (only blindings of at least
/// <see cref="MinCountedSeconds"/> count, shorter ones are glances) with their average duration, and teammates blinded.
/// Self-flashes are ignored.</summary>
public static class FlashAnalyzer
{
	#region Public Fields

	/// <summary>Shortest blinding that counts as a flash worth mentioning.</summary>
	public const float MinCountedSeconds = 0.5f;

	#endregion

	#region Public Methods

	/// <summary>Rolls the timeline's blind events up per thrower on our team.</summary>
	public static FlashSummaryDto Analyze(AnalysisContext context)
	{
		var blinds = context.Timeline.Blinds;
		if (blinds.Count == 0)
		{
			return new FlashSummaryDto(false, 0, 0, []);
		}

		var rows = blinds
			.Where(b => b.Attacker.SteamId64 != b.Victim.SteamId64 && context.IsOurs(b.Attacker.SteamId64) && b.DurationSeconds >= MinCountedSeconds)
			.GroupBy(b => b.Attacker.SteamId64)
			.Select(g =>
			{
				var enemies = g.Where(b => !b.IsTeamFlash).ToList();
				var team = g.Where(b => b.IsTeamFlash).ToList();

				return new FlashPlayerDto(
					g.Key.ToString(),
					context.Name(g.Key),
					enemies.Count,
					enemies.Count == 0 ? 0 : Math.Round(enemies.Average(b => b.DurationSeconds), 2),
					team.Count,
					Math.Round(team.Sum(b => b.DurationSeconds), 2));
			})
			.OrderByDescending(p => p.EnemiesFlashed).ThenBy(p => p.Name)
			.ToList();

		return new FlashSummaryDto(true, rows.Sum(p => p.EnemiesFlashed), rows.Sum(p => p.TeamFlashes), rows);
	}

	#endregion
}
