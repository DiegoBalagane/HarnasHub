using FluentValidation;

namespace HarnasHub.Application.Features.Availability.SetVacation;

/// <summary>Validation rules for <see cref="SetVacationCommand"/>.</summary>
public class SetVacationCommandValidator : AbstractValidator<SetVacationCommand>
{
	#region Constructors

	public SetVacationCommandValidator()
	{
		RuleFor(x => x.StartDate).NotEqual(default(DateOnly)).WithMessage("Nieprawidłowa data początkowa.");

		RuleFor(x => x.EndDate)
			.NotEqual(default(DateOnly)).WithMessage("Nieprawidłowa data końcowa.")
			.GreaterThanOrEqualTo(x => x.StartDate)
			.WithMessage("Data końcowa nie może być wcześniejsza niż data początkowa.");

		RuleFor(x => x.Reason).MaximumLength(300).WithMessage("Powód może mieć maksymalnie 300 znaków.");
	}

	#endregion
}
