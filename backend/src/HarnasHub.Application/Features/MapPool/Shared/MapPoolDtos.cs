namespace HarnasHub.Application.Features.MapPool.Shared;

/// <summary>One map of the pool: the coach's classification plus how the team actually does on it.
/// <paramref name="Status"/> is the <c>MapPoolStatus</c> name, null when the map hasn't been classified;
/// <paramref name="WinRatePercentage"/> is null when nothing was played; <paramref name="RecentForm"/> is up to the
/// last five outcomes ("W"/"L"/"D"), newest first.</summary>
public record MapPoolMapDto(
	string MapName,
	string? Status,
	string? Note,
	int Wins,
	int Losses,
	int Draws,
	double? WinRatePercentage,
	List<string> RecentForm,
	DateTime? LastPlayedAtUtc,
	int TacticCount);
