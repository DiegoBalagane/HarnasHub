using FluentValidation;

namespace HarnasHub.Application.Features.Roster.SetVisibility;

/// <summary>Validation rules for <see cref="SetVisibilityCommand"/>.</summary>
public class SetVisibilityCommandValidator : AbstractValidator<SetVisibilityCommand>
{
	#region Constructors

	public SetVisibilityCommandValidator()
	{
		RuleFor(x => x.UserId).NotEmpty().WithMessage("Nieprawidłowy zawodnik.");
	}

	#endregion
}
