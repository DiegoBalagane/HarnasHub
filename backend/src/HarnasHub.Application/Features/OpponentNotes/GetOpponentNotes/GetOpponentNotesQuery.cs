using ErrorOr;
using HarnasHub.Application.Features.OpponentNotes.Shared;
using MediatR;

namespace HarnasHub.Application.Features.OpponentNotes.GetOpponentNotes;

/// <summary>Returns scouting notes, optionally filtered by opponent name.</summary>
public record GetOpponentNotesQuery(string? OpponentName) : IRequest<ErrorOr<List<OpponentNoteDto>>>;
