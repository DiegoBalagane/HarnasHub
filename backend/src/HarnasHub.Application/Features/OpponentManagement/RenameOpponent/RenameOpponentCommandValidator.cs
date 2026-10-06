using FluentValidation;

namespace HarnasHub.Application.Features.OpponentManagement.RenameOpponent;

/// <summary>Validation rules for <see cref="RenameOpponentCommand"/>; the 100-character cap matches the name columns.</summary>
public class RenameOpponentCommandValidator : AbstractValidator<RenameOpponentCommand>
{
	#region Constructors

	public RenameOpponentCommandValidator()
	{
		RuleFor(x => x.From)
			.NotEmpty().WithMessage("Nazwa przeciwnika jest wymagana.")
			.MaximumLength(100).WithMessage("Nazwa przeciwnika może mieć maksymalnie 100 znaków.");

		RuleFor(x => x.To)
			.NotEmpty().WithMessage("Nowa nazwa przeciwnika jest wymagana.")
			.MaximumLength(100).WithMessage("Nowa nazwa przeciwnika może mieć maksymalnie 100 znaków.");
	}

	#endregion
}
