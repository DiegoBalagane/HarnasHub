using FluentValidation;
using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Features.Veto.SetEventVeto;

/// <summary>Validation rules for <see cref="SetEventVetoCommand"/>: each pool map at most once, a decider only as the last step.</summary>
public class SetEventVetoCommandValidator : AbstractValidator<SetEventVetoCommand>
{
	#region Constructors

	public SetEventVetoCommandValidator()
	{
		RuleFor(x => x.EventId).NotEmpty().WithMessage("Nieprawidłowy identyfikator wydarzenia.");

		RuleFor(x => x.Steps)
			.NotNull().WithMessage("Lista kroków veto jest wymagana.")
			.Must(steps => steps.Count <= Enum.GetValues<MapName>().Length)
			.WithMessage("Veto nie może mieć więcej kroków niż map w puli.")
			.Must(steps => steps.Select(s => s.MapName).Distinct().Count() == steps.Count)
			.WithMessage("Każda mapa może wystąpić w veto tylko raz.")
			.Must(steps => steps.SkipLast(1).All(s => s.Action != VetoAction.Decider))
			.WithMessage("Decider może być tylko ostatnim krokiem veto.");

		RuleForEach(x => x.Steps).ChildRules(step =>
		{
			step.RuleFor(s => s.Actor).IsInEnum().WithMessage("Nieprawidłowa strona veto.");
			step.RuleFor(s => s.Action).IsInEnum().WithMessage("Nieprawidłowa akcja veto.");
			step.RuleFor(s => s.MapName).IsInEnum().WithMessage("Nieprawidłowa mapa.");
		});
	}

	#endregion
}
