#region Usings

using FluentValidation;

#endregion

namespace HarnasHub.Application.Features.Tactics.GetTacticEffectiveness;

/// <summary>Validation rules for <see cref="GetTacticEffectivenessQuery"/>.</summary>
public class GetTacticEffectivenessQueryValidator : AbstractValidator<GetTacticEffectivenessQuery>
{
	#region Constructors

	/// <summary>Defines the rules.</summary>
	public GetTacticEffectivenessQueryValidator()
	{
		RuleFor(x => x.Map).IsInEnum().WithMessage("Nieprawidłowa mapa.");
	}

	#endregion
}
