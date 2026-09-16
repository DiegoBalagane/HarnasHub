using FluentValidation;

namespace HarnasHub.Application.Features.Roster.UpdateRosterSlot;

/// <summary>Validation rules for <see cref="UpdateRosterSlotCommand"/>.</summary>
public class UpdateRosterSlotCommandValidator : AbstractValidator<UpdateRosterSlotCommand>
{
	#region Constructors

	public UpdateRosterSlotCommandValidator()
	{
		RuleFor(x => x.UserId).NotEmpty().WithMessage("Nieprawidłowy zawodnik.");

		RuleFor(x => x.NewRosterSlot!.Value)
			.IsInEnum()
			.WithMessage("Nieprawidłowy status w składzie.")
			.When(x => x.NewRosterSlot.HasValue);
	}

	#endregion
}
