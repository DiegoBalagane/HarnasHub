#region Usings

using FluentValidation;

#endregion

namespace HarnasHub.Application.Features.Tactics.ExtractDemoNades;

/// <summary>Validation rules for <see cref="ExtractDemoNadesCommand"/>.</summary>
public class ExtractDemoNadesCommandValidator : AbstractValidator<ExtractDemoNadesCommand>
{
	#region Constructors

	/// <summary>Requires the object key returned by the presigned upload.</summary>
	public ExtractDemoNadesCommandValidator()
	{
		RuleFor(x => x.ObjectKey)
			.NotEmpty().WithMessage("Brak identyfikatora wgranej demki.")
			.MaximumLength(300).WithMessage("Nieprawidłowy identyfikator demki.");
	}

	#endregion
}
