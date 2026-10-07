namespace HarnasHub.Application.Features.Stats.Shared;

/// <summary>The "Zaawansowane" stats page: our players' demo-derived totals plus per-map and per-match breakdowns.</summary>
public record AdvancedStatsDto(
	int MatchesAnalyzed,
	int MatchesSkipped,
	IReadOnlyList<AdvancedPlayerDto> Players,
	IReadOnlyList<AdvancedMapCellDto> MapCells,
	IReadOnlyList<AdvancedFormPointDto> Form);

/// <summary>One player's totals across the counted matches; rating/ADR/KAST are averages of the stored stat lines (null when none had them).</summary>
public record AdvancedPlayerDto(
	Guid UserId,
	string Name,
	int Matches,
	int Rounds,
	int OpeningWonT,
	int OpeningLostT,
	int OpeningWonCt,
	int OpeningLostCt,
	int TradeKills,
	int Deaths,
	int TradedDeaths,
	int ClutchAttempts,
	int ClutchesWon,
	int BestClutchWon,
	int EnemiesFlashed,
	double AvgBlindSeconds,
	int TeamFlashes,
	double? UtilityDamagePerMatch,
	double? AvgKast,
	double? AvgRating,
	double? AvgAdr);

/// <summary>A player's averages on one map (one heatmap cell).</summary>
public record AdvancedMapCellDto(Guid UserId, string Map, int Matches, double AvgRating, double AvgAdr);

/// <summary>A player's rating/ADR in one match, for the form chart.</summary>
public record AdvancedFormPointDto(Guid UserId, Guid MatchResultId, DateTime PlayedAtUtc, string Opponent, string Map, double Rating, double Adr);
