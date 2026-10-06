using FluentValidation;

namespace HarnasHub.Application.Features.OpponentManagement.UnhideOpponent;

/// <summary>Validation rules for <see cref="UnhideOpponentCommand"/>.</summary>
public class UnhideOpponentCommandValidator : AbstractValidator<UnhideOpponentCommand>
{
	#region Constructors

	public UnhideOpponentCommandValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("Nazwa przeciwnika jest wymagana.")
			.MaximumLength(100).WithMessage("Nazwa przeciwnika może mieć maksymalnie 100 znaków.");
	}

	#endregion
}
