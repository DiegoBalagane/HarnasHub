using ErrorOr;

namespace HarnasHub.Application.Features.Availability.Shared;

/// <summary>Domain errors for the Availability feature slice.</summary>
public static class AvailabilityErrors
{
	public static Error VacationNotFound => Error.NotFound(
		"Availability.VacationNotFound",
		"Nie znaleziono urlopu.");

	public static Error NotYourVacation => Error.Forbidden(
		"Availability.NotYourVacation",
		"Możesz usuwać tylko własne urlopy (chyba że jesteś coachem/managerem).");

	public static Error PastDateNotEditable => Error.Validation(
		"Availability.PastDateNotEditable",
		"Nie można edytować dostępności dla minionego dnia (chyba że jesteś coachem/managerem).");
}
