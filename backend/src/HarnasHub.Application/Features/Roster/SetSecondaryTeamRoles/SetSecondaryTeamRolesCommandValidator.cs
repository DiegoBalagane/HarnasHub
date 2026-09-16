using FluentValidation;

namespace HarnasHub.Application.Features.Roster.SetSecondaryTeamRoles;

/// <summary>Validation rules for <see cref="SetSecondaryTeamRolesCommand"/>.</summary>
public class SetSecondaryTeamRolesCommandValidator : AbstractValidator<SetSecondaryTeamRolesCommand>
{
	#region Constructors

	public SetSecondaryTeamRolesCommandValidator()
	{
		RuleFor(x => x.UserId).NotEmpty().WithMessage("Nieprawidłowy zawodnik.");

		RuleForEach(x => x.TeamRoles).IsInEnum().WithMessage("Nieprawidłowa rola w drużynie.");

		RuleFor(x => x.TeamRoles)
			.Must(roles => roles.Count == roles.Distinct().Count())
			.WithMessage("Ta sama rola dodatkowa została podana więcej niż raz.");
	}

	#endregion
}
