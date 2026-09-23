using FluentValidation;

namespace HarnasHub.Application.Features.MapStrategy.AddTextAnnotation;

/// <summary>Validation rules for <see cref="AddTextAnnotationCommand"/>.</summary>
public class AddTextAnnotationCommandValidator : AbstractValidator<AddTextAnnotationCommand>
{
	#region Constructors

	public AddTextAnnotationCommandValidator()
	{
		RuleFor(x => x.MapName).IsInEnum().WithMessage("Nieprawidłowa mapa.");

		RuleFor(x => x.Side).IsInEnum().WithMessage("Nieprawidłowa strona mapy.");

		RuleFor(x => x.Text)
			.NotEmpty().WithMessage("Treść notatki jest wymagana.")
			.MaximumLength(200).WithMessage("Treść notatki może mieć maksymalnie 200 znaków.");

		RuleFor(x => x.Color)
			.Matches("^#[0-9A-Fa-f]{6}$").WithMessage("Kolor musi być w formacie szesnastkowym, np. #ff0000.");

		RuleFor(x => x.FontSizePx)
			.InclusiveBetween(10, 40).WithMessage("Rozmiar czcionki musi mieścić się w zakresie od 10 do 40.");

		RuleFor(x => x.X)
			.InclusiveBetween(0f, 1f).WithMessage("Pozycja X musi mieścić się w zakresie od 0 do 1.");

		RuleFor(x => x.Y)
			.InclusiveBetween(0f, 1f).WithMessage("Pozycja Y musi mieścić się w zakresie od 0 do 1.");
	}

	#endregion
}
