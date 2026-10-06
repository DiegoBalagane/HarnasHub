#region Usings

using System.Text.RegularExpressions;
using FluentValidation;

#endregion

namespace HarnasHub.Application.Features.Results.AnalyzeDemoFromStorage;

/// <summary>Validation rules for <see cref="AnalyzeDemoFromStorageCommand"/>.</summary>
public partial class AnalyzeDemoFromStorageCommandValidator : AbstractValidator<AnalyzeDemoFromStorageCommand>
{
	#region Constructors

	/// <summary>Requires an object key in the exact shape the presigned demo upload hands out (the server reads and then
	/// deletes it, so no other object may be addressable) and caps the original file name.</summary>
	public AnalyzeDemoFromStorageCommandValidator()
	{
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
