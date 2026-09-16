using FluentValidation;

namespace HarnasHub.Application.Features.Roster.UpdateOwnPinColor;

/// <summary>Validation rules for <see cref="UpdateOwnPinColorCommand"/>.</summary>
public class UpdateOwnPinColorCommandValidator : AbstractValidator<UpdateOwnPinColorCommand>
{
	#region Constructors

	public UpdateOwnPinColorCommandValidator()
	{
		RuleFor(x => x.NewPinColor!.Value)
			.IsInEnum()
			.WithMessage("Nieprawidłowy kolor.")
			.When(x => x.NewPinColor.HasValue);
	}

	#endregion
}
