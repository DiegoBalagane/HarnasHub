using ErrorOr;

namespace HarnasHub.Application.Features.Results.Shared;

/// <summary>Domain errors for the Results feature slice.</summary>
public static class ResultErrors
{
	public static Error InvalidDemoFile => Error.Validation(
		"Results.InvalidDemoFile",
		"Nie udało się odczytać tego pliku jako demki CS2 — sprawdź, czy to poprawny plik .dem.");

	public static Error ScoreRequired => Error.Validation(
		"Results.ScoreRequired",
		"Podaj wynik ręcznie albo przeanalizuj demkę i wybierz, która drużyna jest Waszą.");
}
