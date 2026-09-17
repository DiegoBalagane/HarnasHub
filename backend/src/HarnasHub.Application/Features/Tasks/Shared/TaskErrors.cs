using ErrorOr;

namespace HarnasHub.Application.Features.Tasks.Shared;

/// <summary>Domain errors for the Tasks feature slice.</summary>
public static class TaskErrors
{
	public static Error TaskNotFound => Error.NotFound(
		"Tasks.TaskNotFound",
		"Nie znaleziono zadania.");

	public static Error AssigneeNotFound => Error.Validation(
		"Tasks.AssigneeNotFound",
		"Wybrany zawodnik nie istnieje.");

	public static Error MaterialNotFound => Error.Validation(
		"Tasks.MaterialNotFound",
		"Wybrany materiał nie istnieje.");

	public static Error NotYourTask => Error.Forbidden(
		"Tasks.NotYourTask",
		"To zadanie nie jest przypisane do Ciebie.");
}
