using FluentValidation;

namespace HarnasHub.Application.Features.OpponentReport.GetOpponentReport;

/// <summary>Validation rules for <see cref="GetOpponentReportQuery"/>.</summary>
public class GetOpponentReportQueryValidator : AbstractValidator<GetOpponentReportQuery>
{
	#region Constructors

	public GetOpponentReportQueryValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("Nazwa przeciwnika jest wymagana.")
			.MaximumLength(100).WithMessage("Nazwa przeciwnika może mieć maksymalnie 100 znaków.");
	}

	#endregion
}
