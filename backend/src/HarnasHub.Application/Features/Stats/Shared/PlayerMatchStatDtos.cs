namespace HarnasHub.Application.Features.Stats.Shared;

/// <summary>One player's stat line for one match, with their display name for the UI.
/// <paramref name="UserId"/> is null for a demo-imported player nobody on the roster has claimed with a matching SteamID64 yet —
/// <paramref name="DisplayName"/> still resolves to something showable (the demo's own name for them) in that case.
/// Everything from <paramref name="EntryKills"/> onward is only ever set on a row that came from a demo import — null on a manually entered row.</summary>
public record PlayerMatchStatDto(
	Guid Id,
	Guid? UserId,
	string DisplayName,
	int Kills,
	int Deaths,
	int Assists,
	double Adr,
	double HeadshotPercentage,
	double Rating,
	int? EntryKills,
	int? EntryDeaths,
	double? KastPercentage,
	int? MultiKill2K,
	int? MultiKill3K,
	int? MultiKill4K,
	int? MultiKill5K,
	int? UtilityDamage,
	int? FlashAssists,
	IReadOnlyList<DeathPositionDto> DeathPositions);

/// <summary>One player's death, as a radar-relative fraction in [0,1] (see <c>MapPositionAssignment.X/Y</c>) plus which side they were on.</summary>
public record DeathPositionDto(float X, float Y, string Side);

/// <summary>One player's stat line as computed from a parsed demo, before a Coach/Manager reviews and saves it.
/// <paramref name="MatchedUserId"/> is set only when <paramref name="SteamId64"/> matches a roster member's own SteamID64 (see <c>User.SteamId64</c>);
/// unmatched rows are shown for manual assignment instead of being silently dropped. <paramref name="SteamId64"/> is a string,
/// like <c>User.SteamId64</c> — it exceeds Number.MAX_SAFE_INTEGER and would lose precision as a JSON number.</summary>
public record ParsedPlayerStatDto(
	string SteamId64,
	string DemoPlayerName,
	Guid? MatchedUserId,
	string? MatchedDisplayName,
	int Kills,
	int Deaths,
	int Assists,
	double Adr,
	double HeadshotPercentage,
	double Rating,
	int EntryKills,
	int EntryDeaths,
	double KastPercentage,
	int MultiKill2K,
	int MultiKill3K,
	int MultiKill4K,
	int MultiKill5K,
	int UtilityDamage,
	int FlashAssists,
	List<DeathPositionDto> DeathPositions);

/// <summary>The full outcome of parsing one demo: how many rounds it covered, which map it was played on (null if the
/// demo's map isn't in the current pool — the death-map view has nothing to draw on in that case), and every participant's computed stat line.</summary>
public record ImportStatsFromDemoResultDto(int RoundsPlayed, string? MapName, List<ParsedPlayerStatDto> Players);
