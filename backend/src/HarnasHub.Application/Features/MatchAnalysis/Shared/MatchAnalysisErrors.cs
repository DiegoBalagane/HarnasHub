#region Usings

using ErrorOr;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.Shared;

/// <summary>Domain errors for the MatchAnalysis feature slice.</summary>
public static class MatchAnalysisErrors
{
	/// <summary>The match has no stored demo timeline yet.</summary>
	public static Error TimelineNotFound => Error.NotFound(
		"MatchAnalysis.TimelineNotFound",
		"Ten mecz nie ma jeszcze osi czasu — dołącz demkę, aby ją utworzyć.");

	/// <summary>The stored timeline couldn't be read back from object storage.</summary>
	public static Error TimelineUnavailable => Error.Failure(
		"MatchAnalysis.TimelineUnavailable",
		"Nie udało się wczytać osi czasu meczu z magazynu plików. Spróbuj ponownie później.");

	/// <summary>The timeline has no round with the requested number.</summary>
	public static Error RoundNotFound => Error.NotFound(
		"MatchAnalysis.RoundNotFound",
		"Ta demka nie zawiera rundy o podanym numerze.");

	/// <summary>The parsed timeline couldn't be written to object storage / the database.</summary>
	public static Error TimelineSaveFailed => Error.Failure(
		"MatchAnalysis.TimelineSaveFailed",
		"Demka została przeanalizowana, ale nie udało się zapisać osi czasu meczu. Spróbuj ponownie później.");
}
