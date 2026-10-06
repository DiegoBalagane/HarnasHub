using HarnasHub.Application.Features.Calendar.Shared;

namespace HarnasHub.Application.Features.OpponentNotes.Shared;

/// <summary>A scouting note about an opponent.</summary>
public record OpponentNoteDto(Guid Id, string OpponentName, string Content, string? MaterialUrl, DateTime CreatedAtUtc);

/// <summary>One opponent as listed on /opponents — every team name seen in notes, results or events, with the head-to-head record;
/// <paramref name="NextEventAtUtc"/> is the next scheduled game against them, null when none is planned.</summary>
public record OpponentSummaryDto(
	string Name,
	int NoteCount,
	int Wins,
	int Losses,
	int Draws,
	DateTime? LastPlayedAtUtc,
	DateTime? NextEventAtUtc);

/// <summary>Head-to-head record against an opponent on a single map.</summary>
public record OpponentMapRecordDto(string MapName, int Wins, int Losses, int Draws);

/// <summary>One logged game against an opponent; <paramref name="Category"/> is the <c>MatchCategory</c> name.</summary>
public record OpponentMatchDto(
	Guid MatchResultId,
	DateTime PlayedAtUtc,
	string? MapName,
	int OurScore,
	int OpponentScore,
	string Category,
	string? DemoUrl,
	string? Notes);

/// <summary>Everything the team knows about one opponent: record overall and per map, match history, scouting notes and upcoming games.</summary>
public record OpponentProfileDto(
	string Name,
	int Wins,
	int Losses,
	int Draws,
	List<OpponentMapRecordDto> Maps,
	List<OpponentMatchDto> Matches,
	List<OpponentNoteDto> Notes,
	List<EventDto> UpcomingEvents);
