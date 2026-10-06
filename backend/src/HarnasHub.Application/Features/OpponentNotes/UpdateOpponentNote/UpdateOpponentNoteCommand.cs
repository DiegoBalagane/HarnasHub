using ErrorOr;
using HarnasHub.Application.Features.OpponentNotes.Shared;
using MediatR;

namespace HarnasHub.Application.Features.OpponentNotes.UpdateOpponentNote;

/// <summary>Edits an existing scouting note in place. Coach/Manager only — enforced at the endpoint.</summary>
public record UpdateOpponentNoteCommand(Guid NoteId, string OpponentName, string Content, string? MaterialUrl)
	: IRequest<ErrorOr<OpponentNoteDto>>;
