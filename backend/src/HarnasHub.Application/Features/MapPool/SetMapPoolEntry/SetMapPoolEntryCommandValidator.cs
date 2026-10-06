using FluentValidation;

namespace HarnasHub.Application.Features.MapPool.SetMapPoolEntry;

/// <summary>Validation rules for <see cref="SetMapPoolEntryCommand"/>.</summary>
public class SetMapPoolEntryCommandValidator : AbstractValidator<SetMapPoolEntryCommand>
{
	#region Constructors

	public SetMapPoolEntryCommandValidator()
	{
		RuleFor(x => x.MapName).IsInEnum().WithMessage("Nieprawidłowa mapa.");

		RuleFor(x => x.Status).IsInEnum().WithMessage("Nieprawidłowy status mapy.");

		RuleFor(x => x.Note).MaximumLength(300).WithMessage("Notatka może mieć maksymalnie 300 znaków.");
	}

	#endregion
}
