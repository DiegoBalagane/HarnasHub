#region Usings

using FluentValidation;
using HarnasHub.Application.Features.OpponentReport.Tendencies;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.SetOpponentDemoTeam;

/// <summary>Validation rules for <see cref="SetOpponentDemoTeamCommand"/>.</summary>
public class SetOpponentDemoTeamCommandValidator : AbstractValidator<SetOpponentDemoTeamCommand>
{
	#region Constructors

	/// <summary>Requires the demo id and team "A" or "B".</summary>
	public SetOpponentDemoTeamCommandValidator()
	{
		RuleFor(x => x.Id).NotEmpty().WithMessage("Brak identyfikatora demki.");
		RuleFor(x => x.Team)
			.Must(team => team is OpponentSideDetector.TeamA or OpponentSideDetector.TeamB)
			.WithMessage("Wybierz drużynę A albo B.");
	}

	#endregion
}
