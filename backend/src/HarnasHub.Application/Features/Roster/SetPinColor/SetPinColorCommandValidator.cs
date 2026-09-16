using FluentValidation;

namespace HarnasHub.Application.Features.Roster.SetPinColor;

/// <summary>Validation rules for <see cref="SetPinColorCommand"/>.</summary>
public class SetPinColorCommandValidator : AbstractValidator<SetPinColorCommand>
{
	#region Constructors

	public SetPinColorCommandValidator()
	{
		RuleFor(x => x.UserId).NotEmpty().WithMessage("Nieprawidłowy zawodnik.");

		RuleFor(x => x.NewPinColor!.Value)
			.IsInEnum()
			.WithMessage("Nieprawidłowy kolor.")
			.When(x => x.NewPinColor.HasValue);
	}

	#endregion
}
