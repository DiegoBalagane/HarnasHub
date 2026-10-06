#region Usings

using HarnasHub.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

#endregion

namespace HarnasHub.Application.Common.Demos;

/// <summary>Gives demo players a readable name: the demo's own name when it has one, otherwise our roster, otherwise the
/// FACEIT nickname cached for that SteamID64, otherwise "Gracz …1234". Display only — stored data is never rewritten.</summary>
public static class PlayerNameResolver
{
	#region Public Methods

	/// <summary>True when a demo "name" is missing or just a SteamID64 / digits (what the parser yields for unnamed players).</summary>
	public static bool IsUnnamed(string? name) =>
		string.IsNullOrWhiteSpace(name) || name.Trim().All(char.IsDigit);

	/// <summary>Last-resort label built from the last four digits of the SteamID64.</summary>
	public static string Fallback(long steamId64)
	{
		var digits = steamId64.ToString();
		return $"Gracz …{digits[^Math.Min(4, digits.Length)..]}";
	}

	/// <summary>Picks the display name from the already-known sources in priority order; pure, so it is easy to test.</summary>
	public static string Choose(long steamId64, string? demoName, IReadOnlyDictionary<long, string> knownNames) =>
		!IsUnnamed(demoName) ? demoName!.Trim()
		: knownNames.TryGetValue(steamId64, out var known) ? known
		: Fallback(steamId64);

	/// <summary>Loads roster and FACEIT names for the SteamID64s in one query per source (roster wins on conflict).</summary>
	public static async Task<Dictionary<long, string>> LoadKnownNamesAsync(
		IApplicationDbContext dbContext,
		IEnumerable<long> steamIds,
		CancellationToken cancellationToken)
	{
		var wanted = steamIds.Select(id => id.ToString()).Distinct().ToList();
		var known = new Dictionary<long, string>();
		if (wanted.Count == 0)
		{
			return known;
		}

		var roster = await dbContext.Users
			.Where(u => u.SteamId64 != null && wanted.Contains(u.SteamId64))
			.Select(u => new { u.SteamId64, u.InGameNickname, u.DisplayName })
			.ToListAsync(cancellationToken);

		foreach (var user in roster)
		{
			var name = !string.IsNullOrWhiteSpace(user.InGameNickname) ? user.InGameNickname : user.DisplayName;
			if (long.TryParse(user.SteamId64, out var id) && !string.IsNullOrWhiteSpace(name))
			{
				known.TryAdd(id, name.Trim());
			}
		}

		var faceit = await dbContext.FaceitPlayers
			.Where(p => p.SteamId64 != null && wanted.Contains(p.SteamId64))
			.Select(p => new { p.SteamId64, p.Nickname })
			.ToListAsync(cancellationToken);

		foreach (var player in faceit)
		{
			if (long.TryParse(player.SteamId64, out var id) && !string.IsNullOrWhiteSpace(player.Nickname))
			{
				known.TryAdd(id, player.Nickname.Trim());
			}
		}

		return known;
	}

	/// <summary>Resolves a name for every player: <paramref name="demoNames"/> maps SteamID64 → the demo's own name (may be junk).</summary>
	public static async Task<Dictionary<long, string>> ResolveAsync(
		IApplicationDbContext dbContext,
		IReadOnlyDictionary<long, string?> demoNames,
		CancellationToken cancellationToken)
	{
		var unnamed = demoNames.Where(p => IsUnnamed(p.Value)).Select(p => p.Key).ToList();
		var known = await LoadKnownNamesAsync(dbContext, unnamed, cancellationToken);
		return demoNames.ToDictionary(p => p.Key, p => Choose(p.Key, p.Value, known));
	}

	/// <summary>A copy of the timeline with every unnamed player's name replaced by the resolved one (unchanged when all are named).</summary>
	public static async Task<DemoTimeline> ApplyAsync(IApplicationDbContext dbContext, DemoTimeline timeline, CancellationToken cancellationToken)
	{
		var demoNames = new Dictionary<long, string?>();
		foreach (var (id, name) in TimelinePlayerNames.Collect(timeline))
		{
			demoNames[id] = name;
		}

		foreach (var round in timeline.Rounds)
		{
			foreach (var id in round.TerroristSteamIds.Concat(round.CounterTerroristSteamIds))
			{
				demoNames.TryAdd(id, null);
			}
		}

		if (!demoNames.Any(p => IsUnnamed(p.Value)))
		{
			return timeline;
		}

		var names = await ResolveAsync(dbContext, demoNames, cancellationToken);
		return Rename(timeline, names);
	}

	/// <summary>Replaces names in the timeline's stats, kills, blinds, grenades, economy and bomb events.</summary>
	public static DemoTimeline Rename(DemoTimeline timeline, IReadOnlyDictionary<long, string> names)
	{
		string Pick(long id, string current) => IsUnnamed(current) && names.TryGetValue(id, out var n) ? n : current;
		DemoKillParticipant? Part(DemoKillParticipant? p) => p is null ? null : p with { Name = Pick(p.SteamId64, p.Name) };
		DemoBombEvent? Bomb(DemoBombEvent? b) =>
			b is { PlayerSteamId64: { } id } ? b with { PlayerName = Pick(id, b.PlayerName ?? string.Empty) } : b;

		return timeline with
		{
			Stats = timeline.Stats is null ? null : timeline.Stats with
			{
				Players = timeline.Stats.Players.Select(p => p with { PlayerName = Pick(p.SteamId64, p.PlayerName) }).ToList()
			},
			Rounds = timeline.Rounds.Select(r => r with { BombPlant = Bomb(r.BombPlant), BombDefuse = Bomb(r.BombDefuse) }).ToList(),
			Grenades = timeline.Grenades
				.Select(g => g.ThrowerSteamId64 is { } id ? g with { ThrowerName = Pick(id, g.ThrowerName) } : g)
				.ToList(),
			Kills = timeline.Kills.Select(k => k with { Killer = Part(k.Killer), Victim = Part(k.Victim)!, Assister = Part(k.Assister) }).ToList(),
			Blinds = timeline.Blinds.Select(b => b with { Attacker = Part(b.Attacker)!, Victim = Part(b.Victim)! }).ToList(),
			Economy = timeline.Economy
				.Select(e => e with { Players = e.Players.Select(p => p with { Name = Pick(p.SteamId64, p.Name) }).ToList() })
				.ToList()
		};
	}

	#endregion
}
