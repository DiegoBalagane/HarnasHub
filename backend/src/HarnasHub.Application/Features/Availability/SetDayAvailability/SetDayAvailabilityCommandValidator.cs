using FluentValidation;
using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Features.Availability.SetDayAvailability;

/// <summary>Validation rules for <see cref="SetDayAvailabilityCommand"/>.</summary>
public class SetDayAvailabilityCommandValidator : AbstractValidator<SetDayAvailabilityCommand>
{
	#region Constructors

	public SetDayAvailabilityCommandValidator()
	{
		RuleFor(x => x.Date).NotEqual(default(DateOnly)).WithMessage("Nieprawidłowa data.");

		RuleFor(x => x.Status).IsInEnum().WithMessage("Nieprawidłowy status dostępności.");

		RuleFor(x => x.Note).MaximumLength(300).WithMessage("Notatka może mieć maksymalnie 300 znaków.");

		When(x => x.Status == DayAvailabilityStatus.PartiallyAvailable, () =>
		{
			RuleFor(x => x.AvailableFromLocal)
				.NotNull().WithMessage("Podaj godzinę od, gdy jesteś dostępny tylko częściowo.");

			RuleFor(x => x.AvailableToLocal)
				.NotNull().WithMessage("Podaj godzinę do, gdy jesteś dostępny tylko częściowo.");

			RuleFor(x => x)
				.Must(x => x.AvailableFromLocal < x.AvailableToLocal)
				.WithName(nameof(SetDayAvailabilityCommand.AvailableToLocal))
				.WithMessage("Godzina od musi być wcześniejsza niż godzina do.")
				.When(x => x.AvailableFromLocal is not null && x.AvailableToLocal is not null);
		});
	}

	#endregion
}
