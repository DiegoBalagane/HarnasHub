namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>One player's numbers on one pool map over the cached window; rates and share are percentages, K/D a ratio,
/// and the team/solo split follows <see cref="TeamMatchDetector"/> (team = ≥ 3 linked players on the player's side).</summary>
public record PlayerMapFormDto(
	string MapName,
	int Games,
	double Share,
	int Wins,
	double WinRate,
	double KdRatio,
	double? Adr,
	double? HeadshotPercent,
	DateTime LastPlayedAtUtc,
	int TeamGames,
	int SoloGames,
	double? TeamWinRate,
	double? SoloWinRate,
	double? TeamKdRatio,
	double? SoloKdRatio);

/// <summary>Last games vs the earlier ones: K/D and win-rate (percentage points) deltas plus <paramref name="Direction"/>
/// ("Up", "Down" or "Flat").</summary>
public record PlayerRecentFormDto(
	int RecentGames,
	int EarlierGames,
	double RecentKdRatio,
	double EarlierKdRatio,
	double RecentWinRate,
	double EarlierWinRate,
	double KdDelta,
	double WinRateDelta,
	string Direction);

/// <summary>One linked player's individual form across team and solo games; <paramref name="RecentForm"/> is null until there
/// are enough games on both sides of the split.</summary>
public record PlayerFormDto(
	string PlayerId,
	string Nickname,
	int? Elo,
	int? SkillLevel,
	int Games,
	int TeamGames,
	int SoloGames,
	double? WinRate,
	double? KdRatio,
	double? Adr,
	double? HeadshotPercent,
	DateTime? LastPlayedAtUtc,
	PlayerRecentFormDto? RecentForm,
	List<PlayerMapFormDto> Maps);

/// <summary>How comfortable a roster is on a map judging by its players' solo games: of <paramref name="RatedPlayers"/> (players
/// with enough solo games to judge), how many play it regularly and how many avoid it; averages are over the regulars.</summary>
public record MapComfortDto(
	string MapName,
	int RatedPlayers,
	int RegularPlayers,
	int AvoidingPlayers,
	double SoloShare,
	double? AvgWinRate,
	double? AvgKdRatio,
	List<string> RegularNicknames,
	List<string> AvoidingNicknames);

/// <summary>Individual form of one roster: its players and per-map comfort.</summary>
public record TeamIndividualFormDto(List<PlayerFormDto> Players, List<MapComfortDto> MapComfort);

/// <summary>Individual form of both rosters (theirs and ours) next to the team-game report.</summary>
public record IndividualFormDto(TeamIndividualFormDto Theirs, TeamIndividualFormDto Ours);
