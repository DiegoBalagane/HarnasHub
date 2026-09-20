namespace HarnasHub.Application.Features.Results.Shared;

/// <summary>One of the two groups a demo's round-1 sides split into, with the match score it would produce if this
/// is the coach's own team — computed the same way whichever group ends up picked, so switching the pick in the UI
/// is just switching which of these two the form displays.</summary>
public record DemoTeamPreviewDto(IReadOnlyList<string> PlayerNames, int OurScore, int OpponentScore);

/// <summary>One demo participant's raw totals, carried back to the client so a later <c>AddResult</c> call can save a
/// stat line for them without re-uploading the (possibly 100-300MB) demo file a second time.</summary>
public record AnalyzedDemoPlayerDto(
	string SteamId64,
	string DemoPlayerName,
	int Kills,
	int Deaths,
	int Assists,
	int Headshots,
	int DamageDealt,
	int EntryKills,
	int EntryDeaths,
	int KastRounds,
	int UtilityDamage,
	int FlashAssists,
	int MultiKill2K,
	int MultiKill3K,
	int MultiKill4K,
	int MultiKill5K);

/// <summary>What analysing an uploaded demo could tell the coach before they commit to logging the result — the map,
/// both possible team splits with their would-be score, which split (if any) the current roster's SteamID64s suggest
/// is "ours", and every parsed player's raw totals for the eventual stat import.</summary>
public record AnalyzeDemoResultDto(
	int RoundsPlayed,
	string? MapName,
	DemoTeamPreviewDto TeamA,
	DemoTeamPreviewDto TeamB,
	string? SuggestedTeam,
	IReadOnlyList<AnalyzedDemoPlayerDto> Players);
