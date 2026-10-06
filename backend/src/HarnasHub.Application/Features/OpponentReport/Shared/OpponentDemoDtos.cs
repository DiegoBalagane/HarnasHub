namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>One team of a demo as of round 1: "A" started T, "B" started CT.</summary>
public record DemoTeamDto(string Team, List<string> SteamIds, List<string> Names);

/// <summary>One analysed opponent demo. <paramref name="TeamResolved"/> is false when the opponent's team couldn't be
/// detected — the coach then picks <paramref name="OpponentTeam"/> from <paramref name="Teams"/>.</summary>
public record OpponentDemoDto(
	Guid Id,
	string? MapName,
	string? RawMapName,
	DateTime? PlayedAtUtc,
	string Source,
	string? FaceitMatchId,
	int RoundsCount,
	bool TeamResolved,
	string? OpponentTeam,
	List<DemoTeamDto> Teams,
	DateTime CreatedAtUtc);

/// <summary>The opponent's demo list plus what the UI may offer: uploads need object storage, the automatic download
/// needs the FACEIT Downloads API token, the Data API key and a FACEIT link.</summary>
public record OpponentDemosDto(bool StorageConfigured, bool AutoDownloadAvailable, List<OpponentDemoDto> Demos);

/// <summary>Outcome of an automatic FACEIT download run.</summary>
public record OpponentDemoDownloadResultDto(int Analysed, int Failed, int Candidates, List<OpponentDemoDto> Demos);
