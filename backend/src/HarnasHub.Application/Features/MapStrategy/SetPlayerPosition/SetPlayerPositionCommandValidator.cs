using FluentValidation;

namespace HarnasHub.Application.Features.MapStrategy.SetPlayerPosition;

/// <summary>Validation rules for <see cref="SetPlayerPositionCommand"/>.</summary>
public class SetPlayerPositionCommandValidator : AbstractValidator<SetPlayerPositionCommand>
{
	#region Constructors

	public SetPlayerPositionCommandValidator()
	{
		RuleFor(x => x.MapName).IsInEnum().WithMessage("Nieprawidłowa mapa.");

		RuleFor(x => x.Side).IsInEnum().WithMessage("Nieprawidłowa strona mapy.");

		RuleFor(x => x.UserId).NotEmpty().WithMessage("Zawodnik jest wymagany.");

		RuleFor(x => x.X)
			.InclusiveBetween(0f, 1f).WithMessage("Pozycja X musi mieścić się w zakresie od 0 do 1.");

		RuleFor(x => x.Y)
			.InclusiveBetween(0f, 1f).WithMessage("Pozycja Y musi mieścić się w zakresie od 0 do 1.");

		RuleFor(x => x.Label)
			.MaximumLength(50).WithMessage("Nazwa miejsca może mieć maksymalnie 50 znaków.");

		RuleFor(x => x.Note)
			.MaximumLength(300).WithMessage("Notatka może mieć maksymalnie 300 znaków.");
	}

	#endregion
}
