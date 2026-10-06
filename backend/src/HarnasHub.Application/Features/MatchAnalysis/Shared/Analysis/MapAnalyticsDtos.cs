namespace HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;

/// <summary>Rounds won out of rounds played.</summary>
public record WinRateDto(int Won, int Total);

/// <summary>Win rate of our rounds with a given buy type (<paramref name="BuyType"/> is the <c>BuyType</c> name).</summary>
public record BuyTypeWinRateDto(string BuyType, int Won, int Total);

/// <summary>Bombsite stats: <paramref name="Site"/> is "A", "B" or null when the site couldn't be told; <paramref name="Total"/>
/// is plants (our T side) or opposing plants we had to retake (our CT side), <paramref name="Won"/> the rounds we won.</summary>
public record SiteStatDto(string? Site, int Won, int Total);

/// <summary>Opening duels on one side in one zone, summed over all analysed matches.</summary>
public record MapOpeningZoneDto(string Side, string? Zone, int Won, int Lost);

/// <summary>Trade totals summed over all analysed matches.</summary>
public record MapTradesDto(int OurDeaths, int OurTradedDeaths, int OurTradeKills);

/// <summary>A player's clutch record over all analysed matches.</summary>
public record MapClutcherDto(string SteamId64, string Name, int Attempts, int Won);

/// <summary>Everything the Playbook's map statistics tab shows: aggregates over the last analysed matches on one map,
/// plus rule-based insights. <paramref name="MatchesSkipped"/> counts matches whose timeline couldn't be read or whose
/// team couldn't be resolved.</summary>
public record MapAnalyticsDto(
	string Map,
	bool HasZones,
	int MatchesAnalyzed,
	int MatchesSkipped,
	int RoundsAnalyzed,
	WinRateDto TSide,
	WinRateDto CtSide,
	WinRateDto Pistol,
	IReadOnlyList<BuyTypeWinRateDto> BuyTypes,
	IReadOnlyList<SiteStatDto> TSites,
	IReadOnlyList<SiteStatDto> CtRetakes,
	IReadOnlyList<MapOpeningZoneDto> Openings,
	MapTradesDto Trades,
	IReadOnlyList<MapClutcherDto> TopClutchers,
	IReadOnlyList<MatchInsightDto> Insights);
