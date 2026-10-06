#region Usings

using System.Text.RegularExpressions;
using FluentValidation;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.AnalyzeOpponentDemo;

/// <summary>Validation rules for <see cref="AnalyzeOpponentDemoCommand"/>.</summary>
public partial class AnalyzeOpponentDemoCommandValidator : AbstractValidator<AnalyzeOpponentDemoCommand>
{
	#region Constructors

	/// <summary>Requires the opponent name and an object key in the exact shape the presigned demo upload hands out, so the
	/// server can never be asked to read (and then delete) any other object in the bucket.</summary>
	public AnalyzeOpponentDemoCommandValidator()
	{
		RuleFor(x => x.OpponentName)
			.NotEmpty().WithMessage("Nazwa przeciwnika jest wymagana.")
			.MaximumLength(100).WithMessage("Nazwa przeciwnika może mieć maksymalnie 100 znaków.");

		RuleFor(x => x.ObjectKey)
			.NotEmpty().WithMessage("Brak identyfikatora wgranej demki.")
			.Must(key => key is not null && DemoObjectKeyPattern().IsMatch(key)).WithMessage("Nieprawidłowy identyfikator demki.");

		RuleFor(x => x.FileName)
			.MaximumLength(260).WithMessage("Nazwa pliku demki może mieć maksymalnie 260 znaków.");
	}

	#endregion

	#region Private Methods

	[GeneratedRegex("^demos/[0-9a-f]{32}$")]
	private static partial Regex DemoObjectKeyPattern();

	#endregion
}
