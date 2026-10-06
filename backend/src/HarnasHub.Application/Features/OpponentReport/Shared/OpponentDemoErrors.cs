#region Usings

using ErrorOr;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>Domain errors of the opponent demo slices.</summary>
public static class OpponentDemoErrors
{
	#region Public Properties

	/// <summary>No object storage is configured, so demos can't be uploaded or timelines kept.</summary>
	public static Error StorageNotConfigured => Error.Failure(
		"OpponentDemos.StorageNotConfigured",
		"Magazyn plików nie jest skonfigurowany — wgrywanie demek jest niedostępne.");

	/// <summary>The file couldn't be read as a CS2 demo (or contained no official rounds).</summary>
	public static Error InvalidDemoFile => Error.Validation(
		"OpponentDemos.InvalidDemoFile",
		"Nie udało się odczytać pliku jako demki CS2 albo demka nie zawiera rozegranych rund.");

	/// <summary>The analysed demo doesn't exist.</summary>
	public static Error NotFound => Error.NotFound(
		"OpponentDemos.NotFound",
		"Nie znaleziono tej demki przeciwnika.");

	/// <summary>The timeline couldn't be stored or read back from object storage.</summary>
	public static Error TimelineUnavailable => Error.Failure(
		"OpponentDemos.TimelineUnavailable",
		"Nie udało się zapisać ani odczytać osi czasu demki. Spróbuj ponownie później.");

	/// <summary>The FACEIT Downloads API token isn't configured.</summary>
	public static Error DownloadsNotConfigured => Error.Failure(
		"OpponentDemos.DownloadsNotConfigured",
		"Automatyczne pobieranie demek z FACEIT nie jest skonfigurowane — wgraj demki ręcznie.");

	#endregion
}
