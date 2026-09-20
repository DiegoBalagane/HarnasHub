namespace HarnasHub.Application.Features.Stats.Shared;

/// <summary>One player's stat line for one match, with their display name for the UI.</summary>
public record PlayerMatchStatDto(
	Guid Id,
	Guid UserId,
	string DisplayName,
	int Kills,
	int Deaths,
	int Assists,
	double Adr,
	double HeadshotPercentage,
	double Rating);

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
	double Rating);

/// <summary>The full outcome of parsing one demo: how many rounds it covered, and every participant's computed stat line.</summary>
public record ImportStatsFromDemoResultDto(int RoundsPlayed, List<ParsedPlayerStatDto> Players);
