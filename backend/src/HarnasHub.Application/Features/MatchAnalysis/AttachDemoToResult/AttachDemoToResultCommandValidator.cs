#region Usings

using System.Text.RegularExpressions;
using FluentValidation;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.AttachDemoToResult;

/// <summary>Validation rules for <see cref="AttachDemoToResultCommand"/>.</summary>
public partial class AttachDemoToResultCommandValidator : AbstractValidator<AttachDemoToResultCommand>
{
	#region Constructors

	/// <summary>Requires a match id and an object key in the exact shape the presigned demo upload hands out, so the
	/// server can never be asked to read (and then delete) any other object in the bucket.</summary>
	public AttachDemoToResultCommandValidator()
	{
		RuleFor(x => x.MatchResultId).NotEmpty().WithMessage("Brak identyfikatora meczu.");

		RuleFor(x => x.ObjectKey)
			.NotEmpty().WithMessage("Brak identyfikatora wgranej demki.")
			.Must(key => key is not null && DemoObjectKeyPattern().IsMatch(key)).WithMessage("Nieprawidłowy identyfikator demki.");
	}

	#endregion

	#region Private Methods

	[GeneratedRegex("^demos/[0-9a-f]{32}$")]
	private static partial Regex DemoObjectKeyPattern();

	#endregion
}
