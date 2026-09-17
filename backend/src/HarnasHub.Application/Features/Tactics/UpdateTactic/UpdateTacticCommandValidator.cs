using FluentValidation;

namespace HarnasHub.Application.Features.Tactics.UpdateTactic;

/// <summary>Validation rules for <see cref="UpdateTacticCommand"/>.</summary>
public class UpdateTacticCommandValidator : AbstractValidator<UpdateTacticCommand>
{
	#region Constructors

	public UpdateTacticCommandValidator()
	{
		RuleFor(x => x.Economy).IsInEnum().WithMessage("Nieprawidłowy typ ekonomii.");

		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("Nazwa taktyki jest wymagana.")
			.MaximumLength(100).WithMessage("Nazwa może mieć maksymalnie 100 znaków.");

		RuleFor(x => x.Note)
			.MaximumLength(500).WithMessage("Notatka może mieć maksymalnie 500 znaków.");

		RuleForEach(x => x.Points).ChildRules(point =>
		{
			point.RuleFor(p => p.X).InclusiveBetween(0f, 1f).WithMessage("Pozycja X musi mieścić się w zakresie mapy.");
			point.RuleFor(p => p.Y).InclusiveBetween(0f, 1f).WithMessage("Pozycja Y musi mieścić się w zakresie mapy.");
			point.RuleFor(p => p.Description)
				.MaximumLength(300).WithMessage("Opis punktu może mieć maksymalnie 300 znaków.");
		});
	}

	#endregion
}
