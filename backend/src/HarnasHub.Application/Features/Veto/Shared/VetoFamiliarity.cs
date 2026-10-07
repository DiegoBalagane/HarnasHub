#region Usings

using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Features.Veto.Shared;

/// <summary>A map's place in our ban order: pool bans first, then maps we don't play (or still learn), then familiar maps with real
/// negative evidence, and familiar maps without a reason to ban them last.</summary>
public enum VetoBanTier
{
	/// <summary>Permanent ban in the map pool.</summary>
	PoolBan = 0,
	/// <summary>A map we don't play: outside the coach's pool, or (without a pool) few team games and little solo play.</summary>
	NotPlayed = 1,
	/// <summary>A familiar map with negative evidence beyond solo form that isn't one of our comfortable maps.</summary>
	BanCandidate = 2,
	/// <summary>A familiar map without a reason to ban it (including our comfortable maps the opponent is strong on).</summary>
	Keep = 3
}

/// <summary>Whether the familiarity order applies (<paramref name="Active"/> — only when at least one map is familiar, otherwise
/// "we don't play it" carries no information) and whether any map we don't play is in the pool.</summary>
public record VetoFamiliarityContext(bool Active, bool UnplayedMapsExist)
{
	/// <summary>No familiarity ordering — every map is scored on its own evidence only.</summary>
	public static readonly VetoFamiliarityContext None = new(false, false);
}

/// <summary>Our familiarity with a map: maps we don't play are banned before maps we know, whatever the win rate on a known map; the
/// win rate only orders familiar maps among themselves and the coach's pool status stays the strongest signal.</summary>
public static class VetoFamiliarity
{
	#region Public Fields

	/// <summary>Team games (internal results + FACEIT team games) from which we "play" a map.</summary>
	public const int MinTeamGamesForFamiliarity = 3;

	/// <summary>Regular solo players of our lineup from which we "play" a map individually.</summary>
	public const int MinSoloRegulars = 3;

	#endregion

	#region Public Methods

	/// <summary>Our team games on the map: internal results plus FACEIT team games.</summary>
	public static int TeamGames(MapVetoInput map) => map.Wins + map.Losses + map.Draws + map.OurFaceitGames;

	/// <summary>Whether our lineup plays the map individually: most players regularly solo (3+), or 2 regulars backed by a lot of
	/// lifetime matches; nobody avoiding it. Lifetime alone (old matches) is not enough.</summary>
	public static bool PlaysIndividually(MapComfort? comfort, MapLifetime? lifetime) =>
		comfort is { IsAvoided: false } c
		&& (c.RegularPlayers >= MinSoloRegulars || (c.RegularPlayers >= 2 && lifetime is { IsExperienced: true }));

	/// <summary>Any map in the coach's pool (Core, Playable or Learning — a map being trained is still ours); without a pool, enough team
	/// games or regular individual play. Once the pool exists, a map outside it is never ours, whatever a few games or solo play say.</summary>
	public static bool IsFamiliar(MapVetoInput map) =>
		map.Status is MapPoolStatus.Core or MapPoolStatus.Playable or MapPoolStatus.Learning
		|| (!map.CoachPool && map.Status is null && (TeamGames(map) >= MinTeamGamesForFamiliarity || map.OurPlaysIndividually));

	/// <summary>A map without pool status that we don't play — banned first.</summary>
	public static bool IsUnplayed(MapVetoInput map) => map.Status is null && !IsFamiliar(map);

	/// <summary>One of our comfortable maps: Core in the pool, or a familiar map with enough team games and more wins than losses —
	/// never banned just because the opponent is strong on it.</summary>
	public static bool IsComfortable(MapVetoInput map) =>
		map.Status == MapPoolStatus.Core
		|| (IsFamiliar(map) && TeamGames(map) >= MinTeamGamesForFamiliarity && Wins(map) > Losses(map));

	/// <summary>Marks every map with <see cref="MapVetoInput.CoachPool"/> when the coach has put any map in the pool (Core, Playable or
	/// Learning): the pool is then the source of truth, so a map outside it is banned before any map we train.</summary>
	public static List<MapVetoInput> ApplyCoachPool(IReadOnlyCollection<MapVetoInput> maps) =>
		maps.Any(m => m.Status is MapPoolStatus.Core or MapPoolStatus.Playable or MapPoolStatus.Learning)
			? maps.Select(m => m with { CoachPool = true }).ToList()
			: maps.ToList();

	/// <summary>The context of one veto over <paramref name="maps"/>.</summary>
	public static VetoFamiliarityContext Context(IReadOnlyCollection<MapVetoInput> maps) =>
		new(maps.Any(IsFamiliar), maps.Any(IsUnplayed));

	/// <summary>The map's ban tier; <paramref name="negativeEvidence"/> is a negative score without the solo term.</summary>
	public static VetoBanTier Tier(MapVetoInput map, bool negativeEvidence, VetoFamiliarityContext context)
	{
		if (map.Status == MapPoolStatus.Ban)
		{
			return VetoBanTier.PoolBan;
		}

		if (context.Active && !IsFamiliar(map))
		{
			return VetoBanTier.NotPlayed;
		}

		return negativeEvidence && !IsComfortable(map) ? VetoBanTier.BanCandidate : VetoBanTier.Keep;
	}

	/// <summary>The familiarity reason and the short note that leads the map's note (null when nothing worth saying); also tells
	/// whether the note already covers our record. <paramref name="banned"/> marks a familiar map that still fills a ban slot.</summary>
	public static (string? Reason, string? Note, bool CoversRecord) Explain(
		MapVetoInput map,
		bool negativeEvidence,
		VetoFamiliarityContext context,
		bool banned = false)
	{
		var games = TeamGames(map);
		var noun = VetoNotes.MatchNoun(games);
		if (context.Active && IsUnplayed(map))
		{
			if (map.CoachPool)
			{
				return ("Poza naszą pulą map (trenujemy inne) — ban w pierwszej kolejności", "poza naszą pulą map — ban w pierwszej kolejności", true);
			}

			return ($"Nie gramy tej mapy ({games} {noun} drużynowo, brak statusu w puli) — ban w pierwszej kolejności",
				$"nie gramy tej mapy ({games} {noun}) — ban w pierwszej kolejności", true);
		}

		if (!IsFamiliar(map))
		{
			return (null, null, false);
		}

		if (negativeEvidence && IsComfortable(map))
		{
			var ours = map.Status == MapPoolStatus.Core ? "komfortowa w puli" : $"{Record(map)}, {games} {noun}";
			return ($"Rywal jest tu mocny, ale to też nasza mapa ({ours}) — nie banujemy jej",
				$"ich mocna mapa, ale i nasza ({ours}) — nie banujemy", false);
		}

		if (context.Active && context.UnplayedMapsExist && games >= MinTeamGamesForFamiliarity && Wins(map) + 0.5 * map.Draws < games / 2.0)
		{
			var note = banned
				? $"znamy mapę ({games} {noun}), ale bilans {WinRate(map)}% — ban dopiero po mapach, których nie gramy"
				: $"znamy mapę ({games} {noun}), mimo słabego bilansu {WinRate(map)}% — nie banujemy, dopóki są mapy, których nie gramy";
			return ($"{char.ToUpperInvariant(note[0])}{note[1..]}", note, true);
		}

		return context.Active ? ($"Znamy tę mapę ({Source(map, games, noun)})", null, false) : (null, null, false);
	}

	/// <summary>Our record on the map as "wins-losses(-draws)", internal and FACEIT team games together.</summary>
	public static string Record(MapVetoInput map) => $"{Wins(map)}-{Losses(map)}{(map.Draws > 0 ? $"-{map.Draws}" : "")}";

	#endregion

	#region Private Methods

	/// <summary>Our wins, internal and FACEIT.</summary>
	private static int Wins(MapVetoInput map) => map.Wins + map.OurFaceitWins;

	/// <summary>Our losses, internal and FACEIT.</summary>
	private static int Losses(MapVetoInput map) => map.Losses + map.OurFaceitGames - map.OurFaceitWins;

	/// <summary>Raw win rate in whole percent, draws as half wins.</summary>
	private static int WinRate(MapVetoInput map) =>
		(int)Math.Round(100.0 * (Wins(map) + 0.5 * map.Draws) / Math.Max(1, TeamGames(map)));

	/// <summary>Why we count the map as familiar, in a few words.</summary>
	private static string Source(MapVetoInput map, int games, string noun) =>
		games >= MinTeamGamesForFamiliarity ? $"{games} {noun} drużynowo"
		: map.Status is MapPoolStatus.Core or MapPoolStatus.Playable ? "status w puli map"
		: "większość składu gra ją regularnie solo";

	#endregion
}
