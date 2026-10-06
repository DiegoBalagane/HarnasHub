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
	double? OurSoloPrior = null);

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

	/// <summary>Scores every map, then orders them best first and labels the top as picks and the bottom as bans. Pool bans are
	/// always bans; the remaining ban slots only go to maps with real negative evidence (<see cref="VetoMapScore.BanCandidate"/>), so
	/// a tiny sample or solo form alone never turns a map into a ban.</summary>
	public static List<MapVetoSuggestionDto> Suggest(IEnumerable<MapVetoInput> maps)
	{
		var scored = maps
			.Select(map => (Input: map, Result: VetoMapScorer.Score(map)))
			.OrderByDescending(x => x.Result.Score)
			.ThenBy(x => x.Input.MapName.ToString())
			.ToList();

		var poolBans = scored.Count(x => x.Input.Status == MapPoolStatus.Ban);
		// Top the pool bans up to our ban count, but never ban so much that fewer than PickCount maps stay available.
		var extraBans = Math.Clamp(OurBanCount - poolBans, 0, Math.Max(0, scored.Count - poolBans - PickCount));
		var banned = scored
			.Where(x => x.Input.Status != MapPoolStatus.Ban && x.Result.BanCandidate)
			.TakeLast(extraBans)
			.Select(x => x.Input.MapName)
			.Concat(scored.Where(x => x.Input.Status == MapPoolStatus.Ban).Select(x => x.Input.MapName))
			.ToHashSet();
		var picked = scored
			.Where(x => !banned.Contains(x.Input.MapName) && x.Result.Score > 0)
			.Take(PickCount)
			.Select(x => x.Input.MapName)
			.ToHashSet();

		return scored
			.Select(x => new MapVetoSuggestionDto(
				x.Input.MapName.ToString(),
				x.Result.Score,
				banned.Contains(x.Input.MapName) ? "Ban" : picked.Contains(x.Input.MapName) ? "Pick" : "Neutral",
				x.Result.Reasons)
			{
				Note = x.Result.Note
			})
			.ToList();
	}

	#endregion
}
