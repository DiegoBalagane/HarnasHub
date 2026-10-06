using ErrorOr;

namespace HarnasHub.Application.Features.AnalysisBoards.Shared;

/// <summary>Domain errors for the AnalysisBoards feature slice.</summary>
public static class AnalysisBoardErrors
{
	public static Error BoardNotFound => Error.NotFound(
		"AnalysisBoards.BoardNotFound",
		"Nie znaleziono tablicy.");

	/// <summary>The replayed demo is on a map without a verified radar fit, so positions can't be drawn.</summary>
	public static Error MapUncalibrated => Error.Validation(
		"AnalysisBoards.MapUncalibrated",
		"Mapa niekalibrowana — pozycje niedostępne, nie można utworzyć tablicy z tej rundy.");

	/// <summary>The replayed timeline was parsed without position sampling.</summary>
	public static Error PositionsNotRecorded => Error.Validation(
		"AnalysisBoards.PositionsNotRecorded",
		"Ta demka nie ma zapisanych pozycji graczy — dołącz ją ponownie, aby utworzyć tablicę z rundy.");

	public static Error StorageNotConfigured => Error.Failure(
		"AnalysisBoards.StorageNotConfigured",
		"Wgrywanie własnych zrzutów ekranu nie jest jeszcze skonfigurowane — poproś osobę zarządzającą wdrożeniem o ustawienie magazynu S3.");
}
