using FluentValidation;

namespace HarnasHub.Application.Features.Roster.SetIsCoach;

/// <summary>Validation rules for <see cref="SetIsCoachCommand"/>.</summary>
public class SetIsCoachCommandValidator : AbstractValidator<SetIsCoachCommand>
{
	#region Constructors

	public SetIsCoachCommandValidator()
	{
		RuleFor(x => x.UserId).NotEmpty().WithMessage("Nieprawidłowy zawodnik.");
	}

	#endregion
}
