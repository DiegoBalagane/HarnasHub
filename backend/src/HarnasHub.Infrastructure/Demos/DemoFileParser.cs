using DemoFile;
using DemoFile.Game.Cs;
using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Enums;

namespace HarnasHub.Infrastructure.Demos;

/// <summary>Implements <see cref="IDemoParser"/> against the DemoFile.Net library (github.com/saul/demofile-net) — the only
/// actively maintained C# parser for CS2's Source 2 demo format (the older CS:GO-era parsers don't read it).</summary>
public class DemoFileParser : IDemoParser
{
	#region Public Methods

	public async Task<DemoParseResult> ParseAsync(Stream demoStream, CancellationToken cancellationToken)
	{
		var demo = new CsDemoParser();
		var accumulators = new Dictionary<ulong, PlayerAccumulator>();
		var roundsPlayed = 0;
		var rounds = new List<DemoRoundResult>();

		// Reset every round; folded into each player's totals at RoundEnd.
		var roundKills = new Dictionary<ulong, int>();
		var roundKilledOrAssisted = new HashSet<ulong>();
		var roundDied = new HashSet<ulong>();
		var roundHasEntryEvent = false;

		// player_death's own PlayerPawn reference resolves to a zeroed-out position by the time the event fires
		// (the pawn is already mid-teardown) — so instead we remember each player's position as of their last
		// player_hurt (which fires, including for the fatal hit, while the pawn is still fully valid) and use
		// that snapshot at the moment of death.
		var lastKnownPosition = new Dictionary<ulong, (float X, float Y)>();

		PlayerAccumulator GetOrAddAccumulator(CCSPlayerController player)
		{
			if (!accumulators.TryGetValue(player.SteamID, out var accumulator))
			{
				accumulator = new PlayerAccumulator(player.SteamID, player.PlayerName);
				accumulators[player.SteamID] = accumulator;
			}

			return accumulator;
		}

		demo.Source1GameEvents.RoundStart += _ =>
		{
			roundKills.Clear();
			roundKilledOrAssisted.Clear();
			roundDied.Clear();
			roundHasEntryEvent = false;
		};

		demo.Source1GameEvents.PlayerDeath += e =>
		{
			if (e.Player is { } victim)
			{
				var victimAcc = GetOrAddAccumulator(victim);
				victimAcc.Deaths++;
				roundDied.Add(victim.SteamID);

				if (!roundHasEntryEvent)
				{
					victimAcc.EntryDeaths++;
				}

				var side = victim.CSTeamNum == CSTeamNumber.Terrorist ? MapSide.T : MapSide.CT;
				if (lastKnownPosition.TryGetValue(victim.SteamID, out var position))
				{
					victimAcc.RawDeathPositions.Add((position.X, position.Y, side));
				}
			}

			// Suicides/world kills (Attacker null or the victim itself) don't credit anyone, and don't count as an entry.
			if (e.Attacker is { } attacker && !ReferenceEquals(attacker, e.Player))
			{
				var attackerAcc = GetOrAddAccumulator(attacker);
				attackerAcc.Kills++;
				roundKilledOrAssisted.Add(attacker.SteamID);

				if (e.Headshot)
				{
					attackerAcc.Headshots++;
				}

				roundKills.TryGetValue(attacker.SteamID, out var killsSoFar);
				roundKills[attacker.SteamID] = killsSoFar + 1;

				if (!roundHasEntryEvent)
				{
					roundHasEntryEvent = true;
					attackerAcc.EntryKills++;
				}
			}

			if (e.Assister is { } assister)
			{
				var assisterAcc = GetOrAddAccumulator(assister);
				assisterAcc.Assists++;
				roundKilledOrAssisted.Add(assister.SteamID);

				if (e.Assistedflash)
				{
					assisterAcc.FlashAssists++;
				}
			}
		};

		demo.Source1GameEvents.PlayerHurt += e =>
		{
			if (e.Attacker is { } attacker && !ReferenceEquals(attacker, e.Player))
			{
				var accumulator = GetOrAddAccumulator(attacker);
				accumulator.DamageDealt += e.DmgHealth;

				if (e.Weapon is "hegrenade" or "inferno" or "molotov")
				{
					accumulator.UtilityDamage += e.DmgHealth;
				}
			}

			if (e.Player is { } victim && e.PlayerPawn is { } victimPawn)
			{
				lastKnownPosition[victim.SteamID] = (victimPawn.Origin.X, victimPawn.Origin.Y);
			}
		};

		demo.Source1GameEvents.RoundEnd += e =>
		{
			roundsPlayed++;

			var winnerSide = e.Winner switch
			{
				(int)CSTeamNumber.Terrorist => MapSide.T,
				(int)CSTeamNumber.CounterTerrorist => MapSide.CT,
				_ => (MapSide?)null
			};

			// A round the demo doesn't attribute to either side (warmup/aborted) still counts towards the
			// per-player bookkeeping below, but can't contribute to the score.
			if (winnerSide is { } side)
			{
				// Read straight off each player controller's current team rather than tracking player_team events:
				// that event only fires on an actual team *change* (e.g. the halftime swap), so a demo that starts
				// recording after the initial round-1 team joins already happened would see no side data at all
				// until the first swap. The controller's own state is always current, regardless of when the
				// recording started.
				var terrorists = demo.Players.Where(p => p.CSTeamNum == CSTeamNumber.Terrorist).Select(p => (long)p.SteamID).ToList();
				var counterTerrorists = demo.Players.Where(p => p.CSTeamNum == CSTeamNumber.CounterTerrorist).Select(p => (long)p.SteamID).ToList();
				rounds.Add(new DemoRoundResult(side, terrorists, counterTerrorists));
			}

			foreach (var (steamId, kills) in roundKills)
			{
				if (kills >= 2 && accumulators.TryGetValue(steamId, out var accumulator))
				{
					accumulator.MultiKillRounds[Math.Min(kills, 5)]++;
				}
			}

			foreach (var accumulator in accumulators.Values)
			{
				var contributed = roundKilledOrAssisted.Contains(accumulator.SteamId64) || !roundDied.Contains(accumulator.SteamId64);
				if (contributed)
				{
					accumulator.KastRounds++;
				}
			}
		};

		var reader = DemoFileReader.Create(demo, demoStream);
		await reader.ReadAllAsync(cancellationToken);

		// game_newmap only fires on a mid-session map *change*, never for the map a recording starts on — which
		// is every standalone demo — so the server info packet (present from the start) is the reliable source.
		var rawMapName = demo.ServerInfo?.MapName;
		var mapName = rawMapName is null ? null : MapCalibration.ParseMapName(rawMapName);

		var players = accumulators.Values
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

		return new DemoParseResult(roundsPlayed, mapName, players, rounds);
	}

	#endregion

	#region Private Methods

	private static IReadOnlyList<DemoDeathPosition> ResolveDeathPositions(PlayerAccumulator accumulator, MapName? mapName)
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

	#region Private Types

	/// <summary>Mutable running totals for one player while the demo is being read; converted to the immutable <see cref="DemoPlayerStats"/> at the end.</summary>
	private class PlayerAccumulator(ulong steamId64, string playerName)
	{
		public ulong SteamId64 { get; } = steamId64;
		public string PlayerName { get; } = playerName;
		public int Kills { get; set; }
		public int Deaths { get; set; }
		public int Assists { get; set; }
		public int Headshots { get; set; }
		public int DamageDealt { get; set; }
		public int UtilityDamage { get; set; }
		public int EntryKills { get; set; }
		public int EntryDeaths { get; set; }
		public int KastRounds { get; set; }
		public int FlashAssists { get; set; }
		public Dictionary<int, int> MultiKillRounds { get; } = new() { [2] = 0, [3] = 0, [4] = 0, [5] = 0 };
		public List<(float WorldX, float WorldY, MapSide Side)> RawDeathPositions { get; } = [];
	}

	#endregion
}
