using FluentValidation;

namespace HarnasHub.Application.Features.Tactics.CreateTactic;

/// <summary>Validation rules for <see cref="CreateTacticCommand"/>.</summary>
public class CreateTacticCommandValidator : AbstractValidator<CreateTacticCommand>
{
	#region Constructors

	public CreateTacticCommandValidator()
	{
		RuleFor(x => x.MapName).IsInEnum().WithMessage("Nieprawidłowa mapa.");

		RuleFor(x => x.Side).IsInEnum().WithMessage("Nieprawidłowa strona.");

		RuleFor(x => x.Economy).IsInEnum().WithMessage("Nieprawidłowy typ ekonomii.");

		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("Nazwa taktyki jest wymagana.")
			.MaximumLength(100).WithMessage("Nazwa może mieć maksymalnie 100 znaków.");

		RuleFor(x => x.Note)
			.MaximumLength(500).WithMessage("Notatka może mieć maksymalnie 500 znaków.");
	}

	#endregion
}
