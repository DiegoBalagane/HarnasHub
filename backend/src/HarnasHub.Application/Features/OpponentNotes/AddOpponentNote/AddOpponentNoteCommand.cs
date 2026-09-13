using ErrorOr;
using HarnasHub.Application.Features.OpponentNotes.Shared;
using MediatR;

namespace HarnasHub.Application.Features.OpponentNotes.AddOpponentNote;

/// <summary>Adds a scouting note about an opponent. Coach/Manager only — enforced at the endpoint.</summary>
public record AddOpponentNoteCommand(string OpponentName, string Content, string? MaterialUrl) : IRequest<ErrorOr<OpponentNoteDto>>;
