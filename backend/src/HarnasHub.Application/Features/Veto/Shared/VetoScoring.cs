using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Features.Veto.Shared;

/// <summary>Everything the veto score of one map is built from. The pool <paramref name="Status"/> set by the coach is the primary
/// signal; our record (internal results plus <paramref name="OurFaceitGames"/>/<paramref name="OurFaceitWins"/> from the opponent
/// report) and the opponent's strength (<paramref name="TheirGames"/>, <paramref name="TheirWins"/>, recency-weighted smoothed
/// <paramref name="TheirWinRate"/>) only count with a meaningful sample; <paramref name="OurSoloPrior"/> (0.4–0.6, our players' solo
/// form) is a small secondary term.</summary>
public record MapVetoInput(
	MapName MapName,
	MapPoolStatus? Status,
	int Wins,
	int Losses,
	int Draws,
	int HeadToHeadWins,
	int HeadToHeadLosses,
	int OpponentPicks,
	int OpponentBans,
	int TacticCount,
	int OurFaceitGames = 0,
	int OurFaceitWins = 0,
	int TheirGames = 0,
	int TheirWins = 0,
	double? TheirWinRate = null,
	double? OurSoloPrior = null,
	bool OurPlaysIndividually = false);

/// <summary>One map after scoring: its input, score (with ban tier) and the final "Pick"/"Ban"/"Neutral" recommendation.</summary>
public record RankedVetoMap(MapVetoInput Input, VetoMapScore Result, string Recommendation);

/// <summary>Deterministic, explainable veto scoring — deliberately simple additive points rather than a model, so every
/// number on screen has a sentence behind it (see <see cref="VetoMapScorer"/>).</summary>
public static class VetoScoring
{
	#region Public Fields

	/// <summary>How many maps our side bans in a standard BO1 veto of a 7-map pool.</summary>
	public const int OurBanCount = 3;

	/// <summary>How many maps are flagged as picks.</summary>
	public const int PickCount = 2;

	#endregion

	#region Public Methods

	/// <summary>Scores every map and labels picks and bans, best first (maps we don't play and pool bans at the bottom).</summary>
	public static List<MapVetoSuggestionDto> Suggest(IEnumerable<MapVetoInput> maps) => Rank(maps).Select(ToDto).ToList();

	/// <summary>Scores every map with the familiarity context of the whole pool, then labels bans and picks. Pool bans are always
	/// bans; the remaining ban slots go first to maps we don't play (<see cref="VetoBanTier.NotPlayed"/>, incl. Learning), and only
	/// then to familiar maps with real negative evidence (<see cref="VetoBanTier.BanCandidate"/>) — a tiny sample, solo form or
	/// the opponent's strength on one of our comfortable maps never turns a map into a ban. Picks need a familiar map with a positive score.</summary>
	public static List<RankedVetoMap> Rank(IEnumerable<MapVetoInput> maps)
	{
		var inputs = VetoFamiliarity.ApplyCoachPool(maps.ToList());
		var context = VetoFamiliarity.Context(inputs);
		var scored = inputs
			.Select(map => (Input: map, Result: VetoMapScorer.Score(map, context)))
			.OrderByDescending(x => Group(x.Result.Tier))
			.ThenByDescending(x => x.Result.Score)
			.ThenBy(x => x.Input.MapName.ToString())
			.ToList();

		var poolBans = scored.Count(x => x.Result.Tier == VetoBanTier.PoolBan);
		// Top the pool bans up to our ban count, but never ban so much that fewer than PickCount maps stay available.
		var extraBans = Math.Clamp(OurBanCount - poolBans, 0, Math.Max(0, scored.Count - poolBans - PickCount));
		var banned = scored
			.Where(x => x.Result.Tier is VetoBanTier.NotPlayed or VetoBanTier.BanCandidate)
			.OrderBy(x => x.Result.Tier)
			.ThenBy(x => x.Result.Score)
			.ThenBy(x => x.Input.MapName.ToString())
			.Take(extraBans)
			.Select(x => x.Input.MapName)
			.Concat(scored.Where(x => x.Result.Tier == VetoBanTier.PoolBan).Select(x => x.Input.MapName))
			.ToHashSet();
		var picked = scored
			.Where(x => !banned.Contains(x.Input.MapName) && Group(x.Result.Tier) == 2 && x.Result.Score > 0)
			.Take(PickCount)
			.Select(x => x.Input.MapName)
			.ToHashSet();

		return scored
			.Select(x => banned.Contains(x.Input.MapName)
				? new RankedVetoMap(
					x.Input,
					x.Result.Tier == VetoBanTier.BanCandidate ? VetoMapScorer.Score(x.Input, context, banned: true) : x.Result,
					"Ban")
				: new RankedVetoMap(x.Input, x.Result, picked.Contains(x.Input.MapName) ? "Pick" : "Neutral"))
			.ToList();
	}

	/// <summary>The API shape of a ranked map.</summary>
	public static MapVetoSuggestionDto ToDto(RankedVetoMap map) =>
		new(map.Input.MapName.ToString(), map.Result.Score, map.Recommendation, map.Result.Reasons)
		{
			Note = map.Result.Note
		};

	#endregion

	#region Private Methods

	/// <summary>Display group: familiar maps (2) above maps we don't play (1) above pool bans (0).</summary>
	private static int Group(VetoBanTier tier) => tier switch
	{
		VetoBanTier.PoolBan => 0,
		VetoBanTier.NotPlayed => 1,
		_ => 2
	};

	#endregion
}
