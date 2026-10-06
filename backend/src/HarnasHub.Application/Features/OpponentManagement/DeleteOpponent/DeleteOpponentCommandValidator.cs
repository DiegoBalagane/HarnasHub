using FluentValidation;

namespace HarnasHub.Application.Features.OpponentManagement.DeleteOpponent;

/// <summary>Validation rules for <see cref="DeleteOpponentCommand"/>.</summary>
public class DeleteOpponentCommandValidator : AbstractValidator<DeleteOpponentCommand>
{
	#region Constructors

	public DeleteOpponentCommandValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("Nazwa przeciwnika jest wymagana.")
			.MaximumLength(100).WithMessage("Nazwa przeciwnika może mieć maksymalnie 100 znaków.");
	}

	#endregion
}
