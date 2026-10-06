#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Features.Tactics.Shared;

/// <summary>Turns a parsed <see cref="DemoTimeline"/> into the import wizard's round/grenade view — a pure function so
/// the per-side score tracking and grenade filtering are unit-testable without a real demo.</summary>
public static class DemoNadeMapper
{
	#region Public Methods

	/// <summary>Builds the per-round view. Decoys (no library equivalent) and grenades without a radar throw/landing
	/// position or a known side are left out.</summary>
	public static DemoNadesDto Map(MapName mapName, DemoTimeline timeline)
	{
		var grenadesByRound = timeline.Grenades
			.Select(ToDto)
			.OfType<(int Round, DemoNadeDto Dto)>()
			.GroupBy(g => g.Round)
			.ToDictionary(g => g.Key, g => (IReadOnlyList<DemoNadeDto>)g.Select(x => x.Dto).OrderBy(x => x.SecondsIntoRound).ToList());

		var scores = ScoresAfterEachRound(timeline.Rounds);
		var rounds = timeline.Rounds
			.Select((round, index) => new DemoNadeRoundDto(
				round.Number,
				round.WinnerSide,
				scores[index].Terrorist,
				scores[index].CounterTerrorist,
				grenadesByRound.TryGetValue(round.Number, out var grenades) ? grenades : []))
			.ToList();

		return new DemoNadesDto(mapName, rounds);
	}

	/// <summary>Score after every round, per side. A team is identified by the first round's T roster and followed across
	/// halftime/overtime swaps by majority, the same way <c>DemoScoreCalculator</c> finds our side.</summary>
	public static IReadOnlyList<(int Terrorist, int CounterTerrorist)> ScoresAfterEachRound(IReadOnlyList<DemoTimelineRound> rounds)
	{
		var teamA = rounds.Select(r => r.TerroristSteamIds).FirstOrDefault(r => r.Count > 0)?.ToHashSet() ?? [];
		var teamAWins = 0;
		var teamBWins = 0;
		var teamAOnT = true;
		var result = new List<(int, int)>(rounds.Count);

		foreach (var round in rounds)
		{
			var onT = round.TerroristSteamIds.Count(teamA.Contains);
			var onCt = round.CounterTerroristSteamIds.Count(teamA.Contains);
			if (onT > 0 || onCt > 0)
			{
				teamAOnT = onT >= onCt;
			}

			var teamASide = teamAOnT ? MapSide.T : MapSide.CT;
			if (round.WinnerSide == teamASide)
			{
				teamAWins++;
			}
			else if (round.WinnerSide is not null)
			{
				teamBWins++;
			}

			result.Add(teamAOnT ? (teamAWins, teamBWins) : (teamBWins, teamAWins));
		}

		return result;
	}

	/// <summary>Maps the demo's grenade kind onto the library's; null for decoys, which the library has no type for.</summary>
	public static GrenadeType? ToGrenadeType(DemoGrenadeType type) => type switch
	{
		DemoGrenadeType.Smoke => GrenadeType.Smoke,
		DemoGrenadeType.Flash => GrenadeType.Flash,
		DemoGrenadeType.HighExplosive => GrenadeType.Frag,
		DemoGrenadeType.Molotov or DemoGrenadeType.Incendiary => GrenadeType.Molotov,
		_ => null
	};

	#endregion

	#region Private Methods

	private static (int Round, DemoNadeDto Dto)? ToDto(DemoGrenade grenade)
	{
		if (ToGrenadeType(grenade.Type) is not { } type
			|| grenade.ThrowerSide is not { } side
			|| grenade.Throw is not { RadarX: { } throwX, RadarY: { } throwY }
			|| grenade.Landing is not { RadarX: { } landX, RadarY: { } landY })
		{
			return null;
		}

		return (grenade.RoundNumber, new DemoNadeDto(
			grenade.Id,
			type,
			grenade.ThrowerName,
			grenade.ThrowerSteamId64?.ToString(),
			side,
			throwX,
			throwY,
			landX,
			landY,
			MathF.Round(grenade.SecondsIntoRound, 1)));
	}

	#endregion
}
