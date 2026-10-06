#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Infrastructure.Demos.Collectors;

/// <summary>Mutable target every <see cref="IDemoCollector"/> writes its section into once the demo has been read;
/// also the single place world coordinates become radar fractions, since the map is only known for sure by then.</summary>
internal sealed class DemoTimelineBuilder(string? rawMapName, MapName? mapName)
{
	#region Public Properties

	/// <summary>The demo's map within the current pool, or null when it is outside it / unknown.</summary>
	public MapName? MapName { get; } = mapName;

	/// <summary>Per-player totals, set by the stats collector.</summary>
	public DemoParseResult? Stats { get; set; }

	/// <summary>Official rounds, filled by the round collector.</summary>
	public List<DemoTimelineRound> Rounds { get; } = [];

	/// <summary>Thrown grenades, filled by the grenade collector.</summary>
	public List<DemoGrenade> Grenades { get; } = [];

	/// <summary>Every death, filled by the kill collector.</summary>
	public List<DemoKill> Kills { get; } = [];

	/// <summary>Per-round loadouts, filled by the economy collector.</summary>
	public List<DemoRoundEconomy> Economy { get; } = [];

	/// <summary>Flash blindings, filled by the blind collector.</summary>
	public List<DemoBlind> Blinds { get; } = [];

	/// <summary>Per-second position tracks, filled by the position sampler.</summary>
	public List<DemoPlayerTrack> Positions { get; } = [];

	#endregion

	#region Public Methods

	/// <summary>Wraps a world position together with its radar fraction (null fraction when the map has no calibration).</summary>
	public DemoPosition ToPosition(float worldX, float worldY, float worldZ)
	{
		var fraction = MapName is { } map ? MapCalibration.ToRadarFraction(map, worldX, worldY) : null;
		return new DemoPosition(worldX, worldY, worldZ, fraction?.X, fraction?.Y);
	}

	/// <summary>Freezes everything contributed so far into the immutable timeline.</summary>
	public DemoTimeline Build() => new(
		rawMapName,
		MapName,
		MapName is { } map && MapCalibration.IsCalibrated(map),
		Stats,
		Rounds.OrderBy(r => r.Number).ToList(),
		Grenades.OrderBy(g => g.RoundNumber).ThenBy(g => g.SecondsIntoRound).ToList())
	{
		Kills = Kills.OrderBy(k => k.RoundNumber).ThenBy(k => k.SecondsIntoRound).ToList(),
		Economy = Economy.OrderBy(e => e.RoundNumber).ToList(),
		Blinds = Blinds.OrderBy(b => b.RoundNumber).ThenBy(b => b.SecondsIntoRound).ToList(),
		Positions = Positions.OrderBy(t => t.RoundNumber).ThenBy(t => t.StartSecond).ThenBy(t => t.SteamId64).ToList()
	};

	#endregion
}
