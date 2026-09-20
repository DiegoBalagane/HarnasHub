using DemoFile;
using DemoFile.Game.Cs;
using HarnasHub.Application.Abstractions;

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

		PlayerAccumulator GetOrAddAccumulator(CCSPlayerController player)
		{
			if (!accumulators.TryGetValue(player.SteamID, out var accumulator))
			{
				accumulator = new PlayerAccumulator(player.SteamID, player.PlayerName);
				accumulators[player.SteamID] = accumulator;
			}

			return accumulator;
		}

		demo.Source1GameEvents.PlayerDeath += e =>
		{
			if (e.Player is { } victim)
			{
				GetOrAddAccumulator(victim).Deaths++;
			}

			// Suicides/world kills (Attacker null or the victim itself) don't credit anyone.
			if (e.Attacker is { } attacker && !ReferenceEquals(attacker, e.Player))
			{
				var accumulator = GetOrAddAccumulator(attacker);
				accumulator.Kills++;

				if (e.Headshot)
				{
					accumulator.Headshots++;
				}
			}

			if (e.Assister is { } assister)
			{
				GetOrAddAccumulator(assister).Assists++;
			}
		};

		demo.Source1GameEvents.PlayerHurt += e =>
		{
			// Self-damage (fall damage, own nade) shouldn't count toward ADR.
			if (e.Attacker is { } attacker && !ReferenceEquals(attacker, e.Player))
			{
				GetOrAddAccumulator(attacker).DamageDealt += e.DmgHealth;
			}
		};

		demo.Source1GameEvents.RoundEnd += _ => roundsPlayed++;

		var reader = DemoFileReader.Create(demo, demoStream);
		await reader.ReadAllAsync(cancellationToken);

		var players = accumulators.Values
			.Select(a => new DemoPlayerStats((long)a.SteamId64, a.PlayerName, a.Kills, a.Deaths, a.Assists, a.Headshots, a.DamageDealt))
			.ToList();

		return new DemoParseResult(roundsPlayed, players);
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
	}

	#endregion
}
