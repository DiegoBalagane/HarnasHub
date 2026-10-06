using FluentValidation;

namespace HarnasHub.Application.Features.Veto.GetVetoSuggestion;

/// <summary>Validation rules for <see cref="GetVetoSuggestionQuery"/>.</summary>
public class GetVetoSuggestionQueryValidator : AbstractValidator<GetVetoSuggestionQuery>
{
	#region Constructors

	public GetVetoSuggestionQueryValidator()
	{
		RuleFor(x => x.OpponentName)
			.NotEmpty().WithMessage("Nazwa przeciwnika jest wymagana.")
			.MaximumLength(100).WithMessage("Nazwa przeciwnika może mieć maksymalnie 100 znaków.");
	}

	#endregion
}
