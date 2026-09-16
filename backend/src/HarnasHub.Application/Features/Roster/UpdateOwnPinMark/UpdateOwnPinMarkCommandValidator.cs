using System.Globalization;
using FluentValidation;

namespace HarnasHub.Application.Features.Roster.UpdateOwnPinMark;

/// <summary>Validation rules for <see cref="UpdateOwnPinMarkCommand"/>.</summary>
public class UpdateOwnPinMarkCommandValidator : AbstractValidator<UpdateOwnPinMarkCommand>
{
	#region Constructors

	public UpdateOwnPinMarkCommandValidator()
	{
		RuleFor(x => x.NewPinMark)
			.Must(IsSingleTextElement)
			.WithMessage("Znak musi być pojedynczym znakiem (literą, cyfrą lub symbolem).")
			.When(x => !string.IsNullOrEmpty(x.NewPinMark));
	}

	#endregion

	#region Private Methods

	// Counts grapheme clusters rather than UTF-16 code units, so a single emoji (which can span a
	// surrogate pair plus modifiers) still passes as "one character".
	private static bool IsSingleTextElement(string? value)
	{
		if (value is null)
		{
			return true;
		}

		var enumerator = StringInfo.GetTextElementEnumerator(value);
		var elementCount = 0;

		while (enumerator.MoveNext())
		{
			elementCount++;
		}

		return elementCount == 1;
	}

	#endregion
}
