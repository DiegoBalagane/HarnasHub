using FluentValidation;

namespace HarnasHub.Application.Features.OpponentManagement.HideOpponent;

/// <summary>Validation rules for <see cref="HideOpponentCommand"/>.</summary>
public class HideOpponentCommandValidator : AbstractValidator<HideOpponentCommand>
{
	#region Constructors

	public HideOpponentCommandValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("Nazwa przeciwnika jest wymagana.")
			.MaximumLength(100).WithMessage("Nazwa przeciwnika może mieć maksymalnie 100 znaków.");
	}

	#endregion
}
