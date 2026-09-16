using FluentValidation;

namespace HarnasHub.Application.Features.Availability.UpdateVacation;

/// <summary>Validation rules for <see cref="UpdateVacationCommand"/>, mirroring <see cref="SetVacation.SetVacationCommandValidator"/>.</summary>
public class UpdateVacationCommandValidator : AbstractValidator<UpdateVacationCommand>
{
	#region Constructors

	public UpdateVacationCommandValidator()
	{
		RuleFor(x => x.VacationId).NotEmpty().WithMessage("Nieprawidłowy identyfikator urlopu.");

		RuleFor(x => x.StartDate).NotEqual(default(DateOnly)).WithMessage("Nieprawidłowa data początkowa.");

		RuleFor(x => x.EndDate)
			.NotEqual(default(DateOnly)).WithMessage("Nieprawidłowa data końcowa.")
			.GreaterThanOrEqualTo(x => x.StartDate)
			.WithMessage("Data końcowa nie może być wcześniejsza niż data początkowa.");

		RuleFor(x => x.Reason).MaximumLength(300).WithMessage("Powód może mieć maksymalnie 300 znaków.");
	}

	#endregion
}
