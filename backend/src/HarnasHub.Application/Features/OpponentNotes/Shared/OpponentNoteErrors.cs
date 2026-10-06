using ErrorOr;

namespace HarnasHub.Application.Features.OpponentNotes.Shared;

/// <summary>Domain errors for the OpponentNotes feature slice.</summary>
public static class OpponentNoteErrors
{
	/// <summary>The scouting note doesn't exist (or was already deleted).</summary>
	public static Error NoteNotFound => Error.NotFound(
		"OpponentNotes.NoteNotFound",
		"Nie znaleziono notatki.");
}
