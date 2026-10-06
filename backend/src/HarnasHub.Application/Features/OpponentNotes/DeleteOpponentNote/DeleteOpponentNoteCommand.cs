using ErrorOr;
using MediatR;

namespace HarnasHub.Application.Features.OpponentNotes.DeleteOpponentNote;

/// <summary>Permanently removes a scouting note. Coach/Manager only — enforced at the endpoint.</summary>
public record DeleteOpponentNoteCommand(Guid NoteId) : IRequest<ErrorOr<Success>>;
