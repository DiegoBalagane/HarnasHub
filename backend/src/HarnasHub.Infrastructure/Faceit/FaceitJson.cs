using System.Globalization;
using System.Text.Json;
using HarnasHub.Application.Abstractions;

namespace HarnasHub.Infrastructure.Faceit;

/// <summary>Defensive readers for FACEIT Data API payloads. Many fields (match statistics in particular) are untyped in the
/// official schema and arrive as strings ("13", "85.2"), so every value is read leniently and missing data falls back to null/0.</summary>
internal static class FaceitJson
{
	#region Public Methods

	/// <summary>Reads a player profile (GET /players, GET /players/{id}).</summary>
	public static FaceitPlayerInfo? ReadPlayer(JsonElement root)
	{
		var playerId = Str(root, "player_id");
		if (string.IsNullOrWhiteSpace(playerId))
		{
			return null;
		}

		var cs2 = Prop(Prop(root, "games"), "cs2");
		var steamId = Str(cs2, "game_player_id") ?? Str(root, "steam_id_64");
		return new FaceitPlayerInfo(playerId, Str(root, "nickname") ?? playerId, steamId, NullableInt(cs2, "faceit_elo"), NullableInt(cs2, "skill_level"));
	}

	/// <summary>Reads a team with its members (GET /teams/{id}).</summary>
	public static FaceitTeamInfo? ReadTeam(JsonElement root)
	{
		var teamId = Str(root, "team_id");
		if (string.IsNullOrWhiteSpace(teamId))
		{
			return null;
		}

		var members = Items(root, "members")
			.Select(m => (Id: Str(m, "user_id") ?? Str(m, "player_id"), Member: m))
			.Where(m => !string.IsNullOrWhiteSpace(m.Id))
			.Select(m => new FaceitPlayerRef(m.Id!, Str(m.Member, "nickname") ?? m.Id!, null, NullableInt(m.Member, "skill_level")))
			.ToList();
		return new FaceitTeamInfo(teamId, Str(root, "name") ?? Str(root, "nickname") ?? teamId, members);
	}

	/// <summary>Reads a match room with both factions (GET /matches/{id}).</summary>
	public static FaceitMatchInfo? ReadMatch(JsonElement root)
	{
		var matchId = Str(root, "match_id");
		var teams = Prop(root, "teams");
		if (string.IsNullOrWhiteSpace(matchId) || teams.ValueKind != JsonValueKind.Object)
		{
			return null;
		}

		var factions = teams.EnumerateObject()
			.Select(faction => new FaceitFactionInfo(
				Str(faction.Value, "faction_id") ?? faction.Name,
				Str(faction.Value, "name") ?? faction.Name,
				Items(faction.Value, "roster")
					.Where(p => !string.IsNullOrWhiteSpace(Str(p, "player_id")))
					.Select(p => new FaceitPlayerRef(
						Str(p, "player_id")!,
						Str(p, "nickname") ?? Str(p, "player_id")!,
						Str(p, "game_player_id"),
						NullableInt(p, "game_skill_level")))
					.ToList()))
			.ToList();
		var demoUrls = Items(root, "demo_url")
			.Where(url => url.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(url.GetString()))
			.Select(url => url.GetString()!)
			.ToList();
		var pickedMaps = Items(Prop(Prop(root, "voting"), "map"), "pick")
			.Where(map => map.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(map.GetString()))
			.Select(map => map.GetString()!)
			.ToList();
		return new FaceitMatchInfo(matchId, factions)
		{
			DemoUrls = demoUrls,
			CompetitionType = Str(root, "competition_type"),
			CompetitionName = Str(root, "competition_name"),
			StartedAtUtc = UnixTime(root, "started_at"),
			FinishedAtUtc = UnixTime(root, "finished_at"),
			PickedMaps = pickedMaps
		};
	}

	/// <summary>Reads a page of match history (GET /players/{id}/history).</summary>
	public static List<FaceitHistoryItem> ReadHistory(JsonElement root) =>
		Items(root, "items")
			.Where(i => !string.IsNullOrWhiteSpace(Str(i, "match_id")))
			.Select(i => new FaceitHistoryItem(
				Str(i, "match_id")!,
				NullableInt(i, "finished_at") is { } finishedAt ? DateTimeOffset.FromUnixTimeSeconds(finishedAt).UtcDateTime : null,
				Str(i, "competition_type"),
				Str(i, "competition_name"),
				Str(i, "status")))
			.ToList();

	/// <summary>Reads per-map scoreboards (GET /matches/{id}/stats) — one "round" of the response is one map.</summary>
	public static List<FaceitMapStats> ReadMatchStats(JsonElement root) =>
		Items(root, "rounds")
			.Select((round, index) =>
			{
				var roundStats = Prop(round, "round_stats");
				var winnerId = Str(roundStats, "Winner");
				var scoreParts = (Str(roundStats, "Score") ?? "").Split('/', StringSplitOptions.TrimEntries);
				var teams = Items(round, "teams")
					.Select((team, teamIndex) => ReadTeamStats(team, teamIndex, winnerId, scoreParts))
					.ToList();
				var mapNumber = NullableInt(round, "match_round") ?? index + 1;
				return new FaceitMapStats(mapNumber, Str(roundStats, "Map"), teams);
			})
			.Where(map => map.Teams.Count == 2)
			.ToList();

	#endregion

	#region Private Methods

	/// <summary>One team of a map scoreboard; the score falls back to the "13 / 7" round summary when "Final Score" is missing.</summary>
	private static FaceitTeamMapStats ReadTeamStats(JsonElement team, int teamIndex, string? winnerId, string[] scoreParts)
	{
		var teamId = Str(team, "team_id") ?? $"team{teamIndex + 1}";
		var teamStats = Prop(team, "team_stats");
		var score = NullableInt(teamStats, "Final Score")
			?? (scoreParts.Length == 2 && int.TryParse(scoreParts[teamIndex], CultureInfo.InvariantCulture, out var parsed) ? parsed : 0);
		var won = winnerId is not null ? winnerId == teamId : Str(teamStats, "Team Win") == "1";
		var players = Items(team, "players")
			.Where(p => !string.IsNullOrWhiteSpace(Str(p, "player_id")))
			.Select(p =>
			{
				var stats = Prop(p, "player_stats");
				return new FaceitPlayerMapStats(
					Str(p, "player_id")!,
					Str(p, "nickname") ?? Str(p, "player_id")!,
					NullableInt(stats, "Kills") ?? 0,
					NullableInt(stats, "Deaths") ?? 0,
					NullableInt(stats, "Assists") ?? 0,
					Dbl(stats, "ADR"),
					Dbl(stats, "Headshots %"),
					NullableInt(stats, "Triple Kills") ?? 0,
					NullableInt(stats, "Quadro Kills") ?? 0,
					NullableInt(stats, "Penta Kills") ?? 0,
					NullableInt(stats, "MVPs") ?? 0);
			})
			.ToList();
		return new FaceitTeamMapStats(teamId, Str(teamStats, "Team") ?? Str(team, "nickname"), score, won, players);
	}

	/// <summary>A child property, or an undefined element when missing or when <paramref name="element"/> isn't an object.</summary>
	private static JsonElement Prop(JsonElement element, string name) =>
		element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) ? value : default;

	/// <summary>The elements of an array property, empty when it is missing or not an array.</summary>
	private static IEnumerable<JsonElement> Items(JsonElement element, string name)
	{
		var array = Prop(element, name);
		return array.ValueKind == JsonValueKind.Array ? array.EnumerateArray() : [];
	}

	/// <summary>A scalar property as text, whatever its JSON type.</summary>
	private static string? Str(JsonElement element, string name)
	{
		var value = Prop(element, name);
		return value.ValueKind switch
		{
			JsonValueKind.String => value.GetString(),
			JsonValueKind.Number => value.GetRawText(),
			JsonValueKind.True => "1",
			JsonValueKind.False => "0",
			_ => null
		};
	}

	/// <summary>A numeric property (number or numeric string) as a double.</summary>
	private static double? Dbl(JsonElement element, string name) =>
		double.TryParse(Str(element, name), NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : null;

	/// <summary>A Unix-seconds timestamp property as UTC; null when missing, zero or not numeric.</summary>
	private static DateTime? UnixTime(JsonElement element, string name) =>
		Dbl(element, name) is { } seconds && seconds > 0 && seconds < 253402300799
			? DateTimeOffset.FromUnixTimeSeconds((long)seconds).UtcDateTime
			: null;

	/// <summary>A numeric property rounded to an int; null when missing or not numeric.</summary>
	private static int? NullableInt(JsonElement element, string name) =>
		Dbl(element, name) is { } value && value is > int.MinValue and < int.MaxValue ? (int)Math.Round(value) : null;

	#endregion
}
