using FluentValidation;

namespace HarnasHub.Application.Features.Nades.UpdateNadePosition;

/// <summary>Validation rules for <see cref="UpdateNadePositionCommand"/>.</summary>
public class UpdateNadePositionCommandValidator : AbstractValidator<UpdateNadePositionCommand>
{
	#region Constructors

	public UpdateNadePositionCommandValidator()
	{
		RuleFor(x => x.X)
			.InclusiveBetween(0f, 1f).When(x => x.X.HasValue)
			.WithMessage("Pozycja X musi mieścić się w zakresie od 0 do 1.");

		RuleFor(x => x.Y)
			.InclusiveBetween(0f, 1f).When(x => x.Y.HasValue)
			.WithMessage("Pozycja Y musi mieścić się w zakresie od 0 do 1.");

		RuleFor(x => x)
			.Must(x => x.X.HasValue == x.Y.HasValue)
			.WithMessage("Obie współrzędne muszą być ustawione albo obie puste.");
	}

	#endregion
}
