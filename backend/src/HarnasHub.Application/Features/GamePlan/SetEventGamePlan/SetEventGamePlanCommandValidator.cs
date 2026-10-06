using FluentValidation;

namespace HarnasHub.Application.Features.GamePlan.SetEventGamePlan;

/// <summary>Validation rules for <see cref="SetEventGamePlanCommand"/>.</summary>
public class SetEventGamePlanCommandValidator : AbstractValidator<SetEventGamePlanCommand>
{
	#region Public Fields

	/// <summary>Upper bound per list — a plan that links more than this stops being a plan.</summary>
	public const int MaxItemsPerKind = 20;

	#endregion

	#region Constructors

	public SetEventGamePlanCommandValidator()
	{
		RuleFor(x => x.EventId).NotEmpty().WithMessage("Nieprawidłowy identyfikator wydarzenia.");

		RuleFor(x => x.Notes).MaximumLength(4000).WithMessage("Plan może mieć maksymalnie 4000 znaków.");

		RuleFor(x => x.TacticIds)
			.NotNull().WithMessage("Lista taktyk jest wymagana.")
			.Must(ids => ids.Count <= MaxItemsPerKind).WithMessage($"Można podpiąć maksymalnie {MaxItemsPerKind} taktyk.")
			.Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Każdą taktykę można podpiąć tylko raz.");

		RuleFor(x => x.BoardIds)
			.NotNull().WithMessage("Lista tablic jest wymagana.")
			.Must(ids => ids.Count <= MaxItemsPerKind).WithMessage($"Można podpiąć maksymalnie {MaxItemsPerKind} tablic.")
			.Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Każdą tablicę można podpiąć tylko raz.");
	}

	#endregion
}
