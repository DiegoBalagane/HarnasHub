using FluentValidation;

namespace HarnasHub.Application.Features.TeamInfo.ReorderTeamInfoEntries;

/// <summary>Validation rules for <see cref="ReorderTeamInfoEntriesCommand"/>.</summary>
public class ReorderTeamInfoEntriesCommandValidator : AbstractValidator<ReorderTeamInfoEntriesCommand>
{
	#region Constructors

	public ReorderTeamInfoEntriesCommandValidator()
	{
		RuleFor(x => x.OrderedIds)
			.NotEmpty().WithMessage("Lista wpisów jest wymagana.")
			.Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Wpisy na liście nie mogą się powtarzać.");
	}

	#endregion
}
