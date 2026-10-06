namespace HarnasHub.Application.Features.OpponentReport.Tendencies;

/// <summary>One bar of a distribution: <paramref name="Percent"/> of the relevant rounds had <paramref name="Label"/>.</summary>
public record TendencyShareDto(string Label, int Count, double Percent);

/// <summary>A radar point for the mini radars; <paramref name="Area"/> is "A"/"B"/"Mid" or null, <paramref name="Awp"/>
/// marks an AWP holder (CT setup) — false elsewhere.</summary>
public record TendencyPointDto(float X, float Y, string? Area, bool Awp);

/// <summary>Average first-duel point per area, drawn as an arrow from T spawn.</summary>
public record EntryArrowDto(string Area, float X, float Y, int Count, double Percent);

/// <summary>A cluster of the opponent's T-side grenades: type (<c>DemoGrenadeType</c> name), centre, in how many T rounds
/// it appeared (<paramref name="Rounds"/>, <paramref name="PerRoundPercent"/> of all their T rounds) and the area.</summary>
public record GrenadeClusterDto(string Type, float X, float Y, int Throws, int Rounds, double PerRoundPercent, string? Area);

/// <summary>Pistol rounds and what they bought right after losing one (<c>BuyType</c> names).</summary>
public record PistolTendencyDto(
	int PistolRounds,
	int PistolWins,
	List<TendencyShareDto> TPistolTargets,
	int LostPistols,
	List<TendencyShareDto> AfterLostPistolBuys);

/// <summary>T-side tendencies: where and when rounds go, entry arrows, grenade clusters and pistol behaviour.</summary>
public record TSideTendenciesDto(
	int Rounds,
	string Confidence,
	List<TendencyShareDto> Targets,
	List<TendencyShareDto> ExecTiming,
	double? AverageExecSecond,
	List<EntryArrowDto> Entries,
	List<GrenadeClusterDto> GrenadeClusters,
	PistolTendencyDto Pistol);

/// <summary>CT-side tendencies: default setups ("2A-1M-2B"), stacks, setup heatmap, AWP spots, early aggression and the
/// reaction to a plant (<paramref name="PostPlantRounds"/> counts only plants with someone still alive).</summary>
public record CtSideTendenciesDto(
	int Rounds,
	string Confidence,
	List<TendencyShareDto> Setups,
	List<TendencyShareDto> Stacks,
	List<TendencyPointDto> SetupPositions,
	List<TendencyShareDto> AwpAreas,
	List<TendencyPointDto> AwpKillPositions,
	int EarlyKillRounds,
	double EarlyKillPercent,
	int PostPlantRounds,
	int Retakes,
	int Saves,
	int RetakesWon);

/// <summary>One opponent player across the analysed demos; rates are percentages. <paramref name="Role"/> is "AWP",
/// "Entry", "Clutch" or null.</summary>
public record PlayerTendencyDto(
	string SteamId64,
	string Name,
	int Rounds,
	int Kills,
	double EntryRate,
	double? OpeningWinRate,
	int AwpKills,
	double AwpKillShare,
	int ClutchAttempts,
	int ClutchWins,
	string? Role);

/// <summary>An anti-strat suggestion: <paramref name="Side"/> is the opponent's side it is about ("T", "CT" or "Players").</summary>
public record AntiStratSuggestionDto(string Kind, string Side, string Text, string Evidence, string Confidence);

/// <summary>Everything learnt from the opponent's demos on one map ("Tendencje (z N demek, M rund)"); the spawn point is the
/// origin of the entry arrows (null when the map has no zones).</summary>
public record MapTendenciesDto(
	string MapName,
	int Demos,
	int Rounds,
	bool HasZones,
	float? TSpawnX,
	float? TSpawnY,
	TSideTendenciesDto T,
	CtSideTendenciesDto Ct,
	List<PlayerTendencyDto> Players,
	List<AntiStratSuggestionDto> Suggestions);
