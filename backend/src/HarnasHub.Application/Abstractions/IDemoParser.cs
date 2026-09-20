namespace HarnasHub.Application.Abstractions;

/// <summary>Reads a CS2 demo (.dem) stream and folds it into per-player kill/death/damage totals. Implemented in Infrastructure
/// against a third-party parsing library, so Application stays free of that dependency — same inversion pattern as everything else here.</summary>
public interface IDemoParser
{
	Task<DemoParseResult> ParseAsync(Stream demoStream, CancellationToken cancellationToken);
}

/// <summary>Everything extracted from one demo: how many rounds were played, and each participant's raw totals.</summary>
public record DemoParseResult(int RoundsPlayed, IReadOnlyList<DemoPlayerStats> Players);

/// <summary>One player's raw totals from a parsed demo — not yet turned into ADR/HS%/rating, that's the caller's job.</summary>
public record DemoPlayerStats(
	long SteamId64,
	string PlayerName,
	int Kills,
	int Deaths,
	int Assists,
	int Headshots,
	int DamageDealt);
