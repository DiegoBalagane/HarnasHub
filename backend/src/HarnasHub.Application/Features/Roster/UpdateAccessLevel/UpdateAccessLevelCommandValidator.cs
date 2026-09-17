using FluentValidation;

namespace HarnasHub.Application.Features.Roster.UpdateAccessLevel;

/// <summary>Validation rules for <see cref="UpdateAccessLevelCommand"/>.</summary>
public class UpdateAccessLevelCommandValidator : AbstractValidator<UpdateAccessLevelCommand>
{
	#region Constructors

	public UpdateAccessLevelCommandValidator()
	{
		RuleFor(x => x.UserId).NotEmpty().WithMessage("Nieprawidłowy zawodnik.");
		RuleFor(x => x.AccessLevel).IsInEnum().WithMessage("Nieprawidłowy poziom dostępu.");
	}

	#endregion
}
