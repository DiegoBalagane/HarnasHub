namespace HarnasHub.Application.Features.OpponentManagement.Shared;

/// <summary>What deleting an opponent would remove: scouting-only data always goes, results and events only on request.</summary>
public record OpponentDeletePreviewDto(
	int Notes,
	int DemoAnalyses,
	bool HasFaceitLink,
	bool HasReportSnapshot,
	int MatchResults,
	int Events,
	bool IsHidden);

/// <summary>Outcome of deleting an opponent; <paramref name="Hidden"/> is true when history was kept and the opponent was hidden instead.</summary>
public record DeleteOpponentResultDto(
	int DeletedNotes,
	int DeletedDemoAnalyses,
	int DeletedMatchResults,
	int DeletedEvents,
	bool Hidden);

/// <summary>Outcome of renaming/merging an opponent; <paramref name="FaceitDataKept"/> is true when the target already had FACEIT data and its own was kept.</summary>
public record RenameOpponentResultDto(
	string Name,
	int UpdatedNotes,
	int UpdatedMatchResults,
	int UpdatedEvents,
	bool FaceitDataKept);
