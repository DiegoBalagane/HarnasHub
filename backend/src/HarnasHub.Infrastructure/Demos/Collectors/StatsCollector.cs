#region Usings

using DemoFile;
using DemoFile.Game.Cs;
using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Infrastructure.Demos.Collectors;

/// <summary>Per-player kill/death/damage totals plus the per-round winner/roster list the match score is computed from —
/// the original demo-parse output, unchanged in every number. Round lifecycle lives here; the combat handlers are in
/// <c>StatsCollector.Combat.cs</c>.</summary>
internal sealed partial class StatsCollector(CsDemoParser demo, DemoRoundClock clock) : IDemoCollector
{
	#region Private Fields

	private readonly Dictionary<ulong, DemoPlayerAccumulator> _accumulators = [];
	private readonly List<DemoRoundResult> _rounds = [];
	private int _roundsPlayed;

	// Reset every round; folded into each player's totals at round_end.
	private readonly Dictionary<ulong, int> _roundKills = [];
	private readonly HashSet<ulong> _roundKilledOrAssisted = [];
	private readonly HashSet<ulong> _roundDied = [];
	private bool _roundHasEntryEvent;

	// Every alive player's position as of the most recent entity update, so the coordinate read at player_death is at
	// most one tick old. The dying player's own pawn update for that tick already reports them as dead and is skipped,
	// which is exactly what we want: the last position at which they were still alive.
	private readonly Dictionary<ulong, (float X, float Y)> _lastKnownPosition = [];

	// Each player's health as of just before the next hit they take, so damage can be capped at what they actually
	// had left to lose (see OnPlayerHurt).
	private readonly Dictionary<ulong, int> _healthBeforeNextHit = [];

	private const int FullHealth = 100;

	#endregion

	#region Public Methods

	/// <inheritdoc />
	public void Subscribe()
	{
		demo.Source1GameEvents.RoundAnnounceMatchStart += _ => OnMatchStart();
		demo.Source1GameEvents.RoundStart += _ => OnRoundStart();
		demo.Source1GameEvents.RoundEnd += OnRoundEnd;
		demo.Source1GameEvents.PlayerDeath += OnPlayerDeath;
		demo.Source1GameEvents.PlayerHurt += OnPlayerHurt;
		demo.Source1GameEvents.PlayerSpawn += OnPlayerSpawn;
		demo.EntityEvents.CCSPlayerPawn.PostUpdate += OnPlayerPawnUpdate;
	}

	/// <inheritdoc />
	public void Contribute(DemoTimelineBuilder builder)
	{
		var players = _accumulators.Values
			.Select(a => new DemoPlayerStats(
				(long)a.SteamId64,
				a.PlayerName,
				a.Kills,
				a.Deaths,
				a.Assists,
				a.Headshots,
				a.DamageDealt,
				a.EntryKills,
				a.EntryDeaths,
				a.KastRounds,
				a.UtilityDamage,
				a.FlashAssists,
				a.MultiKillRounds,
				ResolveDeathPositions(a, builder.MapName)))
			.ToList();

		builder.Stats = new DemoParseResult(_roundsPlayed, builder.MapName, players, _rounds);
	}

	#endregion

	#region Private Methods

	private void OnMatchStart()
	{
		// The match proper begins here, so anything gathered up to now belongs to warmup, the knife round, or an
		// earlier match on the same recording, and is thrown away. (The clock latches the round in progress as official.)
		_accumulators.Clear();
		_rounds.Clear();
		_roundsPlayed = 0;
		ResetRoundScratch();
	}

	private void OnRoundStart()
	{
		ResetRoundScratch();

		// Cleared per round so a player who somehow produces no pawn update before dying can't inherit a coordinate
		// from a previous round; health falls back to full for the same reason.
		_lastKnownPosition.Clear();
		_healthBeforeNextHit.Clear();
	}

	private void OnRoundEnd(Source1RoundEndEvent e)
	{
		if (!clock.IsOfficialMatchRound)
		{
			return;
		}

		_roundsPlayed++;
		RecordRoundWinner(e);
		FoldRoundIntoTotals();
	}

	private void RecordRoundWinner(Source1RoundEndEvent e)
	{
		// A round the demo doesn't attribute to either side (an aborted round) still counts towards the per-player
		// bookkeeping, but can't contribute to the score.
		if (DemoTeams.WinnerSide(e.Winner) is not { } side)
		{
			return;
		}

		var terrorists = DemoTeams.Roster(demo, CSTeamNumber.Terrorist);
		var counterTerrorists = DemoTeams.Roster(demo, CSTeamNumber.CounterTerrorist);
		_rounds.Add(new DemoRoundResult(side, terrorists, counterTerrorists));
	}

	private void FoldRoundIntoTotals()
	{
		foreach (var (steamId, kills) in _roundKills)
		{
			if (kills >= 2 && _accumulators.TryGetValue(steamId, out var accumulator))
			{
				accumulator.MultiKillRounds[Math.Min(kills, 5)]++;
			}
		}

		foreach (var accumulator in _accumulators.Values)
		{
			var contributed = _roundKilledOrAssisted.Contains(accumulator.SteamId64) || !_roundDied.Contains(accumulator.SteamId64);
			if (contributed)
			{
				accumulator.KastRounds++;
			}
		}
	}

	private void ResetRoundScratch()
	{
		_roundKills.Clear();
		_roundKilledOrAssisted.Clear();
		_roundDied.Clear();
		_roundHasEntryEvent = false;
	}

	private DemoPlayerAccumulator GetOrAddAccumulator(CCSPlayerController player)
	{
		if (!_accumulators.TryGetValue(player.SteamID, out var accumulator))
		{
			accumulator = new DemoPlayerAccumulator(player.SteamID, player.PlayerName);
			_accumulators[player.SteamID] = accumulator;
		}

		return accumulator;
	}

	private static IReadOnlyList<DemoDeathPosition> ResolveDeathPositions(DemoPlayerAccumulator accumulator, MapName? mapName)
	{
		if (mapName is null)
		{
			return [];
		}

		var resolved = new List<DemoDeathPosition>(accumulator.RawDeathPositions.Count);

		foreach (var (worldX, worldY, side) in accumulator.RawDeathPositions)
		{
			var fraction = MapCalibration.ToRadarFraction(mapName.Value, worldX, worldY);
			if (fraction is { } f)
			{
				resolved.Add(new DemoDeathPosition(f.X, f.Y, side));
			}
		}

		return resolved;
	}

	#endregion
}
