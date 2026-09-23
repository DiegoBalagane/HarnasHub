using ErrorOr;

namespace HarnasHub.Application.Features.AnalysisBoards.Shared;

/// <summary>Domain errors for the AnalysisBoards feature slice.</summary>
public static class AnalysisBoardErrors
{
	public static Error BoardNotFound => Error.NotFound(
		"AnalysisBoards.BoardNotFound",
		"Nie znaleziono tablicy.");

	public static Error StorageNotConfigured => Error.Failure(
		"AnalysisBoards.StorageNotConfigured",
		"Wgrywanie własnych zrzutów ekranu nie jest jeszcze skonfigurowane — poproś osobę zarządzającą wdrożeniem o ustawienie magazynu S3.");
}
