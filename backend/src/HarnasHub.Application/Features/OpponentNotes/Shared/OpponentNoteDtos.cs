namespace HarnasHub.Application.Features.OpponentNotes.Shared;

/// <summary>A scouting note about an opponent.</summary>
public record OpponentNoteDto(Guid Id, string OpponentName, string Content, string? MaterialUrl, DateTime CreatedAtUtc);
