#region Usings

using DemoFile;
using DemoFile.Game.Cs;
using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Infrastructure.Demos;

/// <summary>Holds all the mutable bookkeeping for one demo being read and wires it up to the parser's events,
/// so <see cref="DemoFileParser"/> itself stays a thin "open, read, hand back the result" shell.</summary>
internal sealed class DemoParseSession(CsDemoParser demo)
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

	// Whether the round in progress counts towards the match, latched at round_start; null until this recording
	// contains one, in which case the live game-rules state is consulted instead.
	private bool? _latchedRoundIsOfficial;

	private const int FullHealth = 100;

	#endregion

	#region Public Methods

	/// <summary>Subscribes every handler this session needs; call once, before reading the demo.</summary>
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

	/// <summary>Folds everything gathered so far into the immutable parse result for the given map.</summary>
	public DemoParseResult BuildResult(MapName? mapName)
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
				ResolveDeathPositions(a, mapName)))
			.ToList();

		return new DemoParseResult(_roundsPlayed, mapName, players, _rounds);
	}

	#endregion

	#region Private Methods

	/// <summary>Warmup and the knife round must never reach the totals — they inflate kills/damage (warmup respawns
	/// endlessly) and the round count that ADR/KAST/Rating are divided by. Read live off CCSGameRules rather than
	/// latched onto warmup_end, for the same reason team sides are read live in <see cref="OnRoundEnd"/>: a demo can
	/// start recording after those one-shot transitions already happened and would then never observe them.</summary>
	private bool IsOfficialMatchRound() =>
		_latchedRoundIsOfficial ?? demo.GameRules is { WarmupPeriod: false, HasMatchStarted: true };

	private void OnMatchStart()
	{
		// The match proper begins here, so anything gathered up to now belongs to warmup, the knife round, or an
		// earlier match on the same recording, and is thrown away. The round already in progress *is* match round 1:
		// CCSGameRules only flips HasMatchStarted a moment later, so it gets latched as official explicitly.
		_accumulators.Clear();
		_rounds.Clear();
		_roundsPlayed = 0;
		ResetRoundScratch();
		_latchedRoundIsOfficial = true;
	}

	private void OnRoundStart()
	{
		ResetRoundScratch();

		// Cleared per round so a player who somehow produces no pawn update before dying can't inherit a coordinate
		// from a previous round; health falls back to full for the same reason.
		_lastKnownPosition.Clear();
		_healthBeforeNextHit.Clear();

		// Latched here and reused for the whole round: CCSGameRules clears HasMatchStarted the moment a match is
		// decided, which happens *before* the final round's round_end fires — judging that event live would silently
		// drop the very round that won the match from both the score and everyone's totals.
		_latchedRoundIsOfficial = demo.GameRules is { WarmupPeriod: false, HasMatchStarted: true };
	}

	private void OnPlayerPawnUpdate(CCSPlayerPawn pawn)
	{
		if (pawn is { IsAlive: true, Health: > 0, OriginalController: { } controller })
		{
			_lastKnownPosition[controller.SteamID] = (pawn.Origin.X, pawn.Origin.Y);
		}
	}

	private void OnPlayerDeath(Source1PlayerDeathEvent e)
	{
		if (!IsOfficialMatchRound())
		{
			return;
		}

		if (e.Player is { } victim)
		{
			var victimAcc = GetOrAddAccumulator(victim);
			victimAcc.Deaths++;
			_roundDied.Add(victim.SteamID);

			if (!_roundHasEntryEvent)
			{
				victimAcc.EntryDeaths++;
			}

			var side = victim.CSTeamNum == CSTeamNumber.Terrorist ? MapSide.T : MapSide.CT;
			if (_lastKnownPosition.TryGetValue(victim.SteamID, out var position))
			{
				victimAcc.RawDeathPositions.Add((position.X, position.Y, side));
			}
		}

		// Suicides/world kills (Attacker null or the victim itself) don't credit anyone, and don't count as an entry.
		if (e.Attacker is { } attacker && !ReferenceEquals(attacker, e.Player))
		{
			var attackerAcc = GetOrAddAccumulator(attacker);
			attackerAcc.Kills++;
			_roundKilledOrAssisted.Add(attacker.SteamID);

			if (e.Headshot)
			{
				attackerAcc.Headshots++;
			}

			_roundKills.TryGetValue(attacker.SteamID, out var killsSoFar);
			_roundKills[attacker.SteamID] = killsSoFar + 1;

			if (!_roundHasEntryEvent)
			{
				_roundHasEntryEvent = true;
				attackerAcc.EntryKills++;
			}
		}

		if (e.Assister is { } assister)
		{
			var assisterAcc = GetOrAddAccumulator(assister);
			assisterAcc.Assists++;
			_roundKilledOrAssisted.Add(assister.SteamID);

			if (e.Assistedflash)
			{
				assisterAcc.FlashAssists++;
			}
		}
	}

	private void OnPlayerSpawn(Source1PlayerSpawnEvent e)
	{
		if (e.Player is { } player)
		{
			_healthBeforeNextHit[player.SteamID] = FullHealth;
		}
	}

	private void OnPlayerHurt(Source1PlayerHurtEvent e)
	{
		if (e.Player is not { } victim)
		{
			return;
		}

		// player_hurt reports the weapon's *raw* damage, not the health the victim actually lost: an AWP chest shot
		// on a full-health player reports 115 and a Deagle headshot well over 200. Tracking what the victim had left
		// (this event's own Health field is their health after the hit, so it seeds the next one) and capping against
		// it is what keeps ADR comparable to HLTV's — uncapped it ran roughly 20% high on real demos. Tracked for
		// every hit, including world/fall damage that credits nobody, or the running total would drift.
		var healthBefore = _healthBeforeNextHit.TryGetValue(victim.SteamID, out var tracked) ? tracked : FullHealth;
		_healthBeforeNextHit[victim.SteamID] = e.Health;

		if (!IsOfficialMatchRound() || e.Attacker is not { } attacker || ReferenceEquals(attacker, victim))
		{
			return;
		}

		// Friendly fire isn't a contribution to the round, so it stays out of ADR the same way HLTV keeps it out.
		if (attacker.CSTeamNum == victim.CSTeamNum)
		{
			return;
		}

		var damage = Math.Clamp(e.DmgHealth, 0, healthBefore);
		var accumulator = GetOrAddAccumulator(attacker);
		accumulator.DamageDealt += damage;

		if (e.Weapon is "hegrenade" or "inferno" or "molotov")
		{
			accumulator.UtilityDamage += damage;
		}
	}

	private void OnRoundEnd(Source1RoundEndEvent e)
	{
		if (!IsOfficialMatchRound())
		{
			return;
		}

		_roundsPlayed++;
		RecordRoundWinner(e);
		FoldRoundIntoTotals();
	}

	private void RecordRoundWinner(Source1RoundEndEvent e)
	{
		var winnerSide = e.Winner switch
		{
			(int)CSTeamNumber.Terrorist => MapSide.T,
			(int)CSTeamNumber.CounterTerrorist => MapSide.CT,
			_ => (MapSide?)null
		};

		// A round the demo doesn't attribute to either side (an aborted round) still counts towards the per-player
		// bookkeeping, but can't contribute to the score.
		if (winnerSide is not { } side)
		{
			return;
		}

		// Read straight off each player controller's current team rather than tracking player_team events: that event
		// only fires on an actual team *change* (e.g. the halftime swap), so a demo that starts recording after the
		// initial round-1 team joins already happened would see no side data at all until the first swap. The
		// controller's own state is always current, regardless of when the recording started.
		var terrorists = demo.Players.Where(p => p.CSTeamNum == CSTeamNumber.Terrorist).Select(p => (long)p.SteamID).ToList();
		var counterTerrorists = demo.Players.Where(p => p.CSTeamNum == CSTeamNumber.CounterTerrorist).Select(p => (long)p.SteamID).ToList();
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
