#region Usings

using FluentValidation;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.GetOpponentDemos;

/// <summary>Validation rules for <see cref="GetOpponentDemosQuery"/>.</summary>
public class GetOpponentDemosQueryValidator : AbstractValidator<GetOpponentDemosQuery>
{
	#region Constructors

	/// <summary>Requires the opponent name.</summary>
	public GetOpponentDemosQueryValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("Nazwa przeciwnika jest wymagana.")
			.MaximumLength(100).WithMessage("Nazwa przeciwnika może mieć maksymalnie 100 znaków.");
	}

	#endregion
}
