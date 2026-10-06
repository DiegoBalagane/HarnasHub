namespace HarnasHub.Application.Common.Jobs;

/// <summary>How one job execution ended: the serialized result on success, a Polish message on failure.</summary>
public sealed record JobOutcome(bool Succeeded, string? ResultJson, string? ErrorMessage)
{
	#region Public Fields

	/// <summary>Message for exceptions nobody anticipated (details only go to the log).</summary>
	public const string UnexpectedErrorMessage = "Wystąpił nieoczekiwany błąd podczas przetwarzania zadania. Spróbuj ponownie.";

	/// <summary>Message for a job that ran past its time limit.</summary>
	public const string TimeoutMessage = "Zadanie trwało zbyt długo i zostało przerwane. Spróbuj ponownie.";

	/// <summary>Message for a job cut short by a server shutdown.</summary>
	public const string ShutdownMessage = "Zadanie zostało przerwane, bo serwer był zatrzymywany. Uruchom je ponownie.";

	/// <summary>Message for jobs found queued/running after a restart (the in-process queue does not survive one).</summary>
	public const string InterruptedMessage = "Zadanie zostało przerwane przez restart serwera. Uruchom je ponownie.";

	#endregion

	#region Public Methods

	/// <summary>A successful outcome carrying <paramref name="resultJson"/>.</summary>
	public static JobOutcome Success(string resultJson) => new(true, resultJson, null);

	/// <summary>A failed outcome with a Polish <paramref name="message"/>.</summary>
	public static JobOutcome Failure(string message) => new(false, null, message);

	#endregion
}
