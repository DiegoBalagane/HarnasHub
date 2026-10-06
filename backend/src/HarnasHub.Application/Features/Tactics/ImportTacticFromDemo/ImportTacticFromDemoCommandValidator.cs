#region Usings

using FluentValidation;

#endregion

namespace HarnasHub.Application.Features.Tactics.ImportTacticFromDemo;

/// <summary>Validation rules for <see cref="ImportTacticFromDemoCommand"/>.</summary>
public class ImportTacticFromDemoCommandValidator : AbstractValidator<ImportTacticFromDemoCommand>
{
	#region Constants

	/// <summary>Upper bound on grenades in one imported tactic — a round never has more than five players' worth.</summary>
	public const int MaxGrenades = 40;

	#endregion

	#region Constructors

	/// <summary>Same tactic rules as manual creation plus per-grenade range checks.</summary>
	public ImportTacticFromDemoCommandValidator()
	{
		RuleFor(x => x.MapName).IsInEnum().WithMessage("Nieprawidłowa mapa.");

		RuleFor(x => x.Side).IsInEnum().WithMessage("Nieprawidłowa strona.");

		RuleFor(x => x.Economy).IsInEnum().WithMessage("Nieprawidłowy typ ekonomii.");

		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("Nazwa taktyki jest wymagana.")
			.MaximumLength(100).WithMessage("Nazwa może mieć maksymalnie 100 znaków.");

		RuleFor(x => x.Note)
			.MaximumLength(500).WithMessage("Notatka może mieć maksymalnie 500 znaków.");

		RuleFor(x => x.Grenades)
			.NotEmpty().WithMessage("Wybierz co najmniej jeden granat.")
			.Must(g => g is null || g.Count <= MaxGrenades).WithMessage($"Można zaimportować maksymalnie {MaxGrenades} granatów naraz.");

		RuleForEach(x => x.Grenades).ChildRules(grenade =>
		{
			grenade.RuleFor(g => g.Type).IsInEnum().WithMessage("Nieprawidłowy typ granatu.");
			grenade.RuleFor(g => g.ThrowerName)
				.NotEmpty().WithMessage("Brak nicku rzucającego.")
				.MaximumLength(64).WithMessage("Nick rzucającego może mieć maksymalnie 64 znaki.");
			grenade.RuleFor(g => g.ThrowX).InclusiveBetween(0f, 1f).WithMessage("Pozycja rzutu musi mieścić się w zakresie mapy.");
			grenade.RuleFor(g => g.ThrowY).InclusiveBetween(0f, 1f).WithMessage("Pozycja rzutu musi mieścić się w zakresie mapy.");
			grenade.RuleFor(g => g.LandX).InclusiveBetween(0f, 1f).WithMessage("Pozycja lądowania musi mieścić się w zakresie mapy.");
			grenade.RuleFor(g => g.LandY).InclusiveBetween(0f, 1f).WithMessage("Pozycja lądowania musi mieścić się w zakresie mapy.");
			grenade.RuleFor(g => g.SecondsIntoRound)
				.InclusiveBetween(0f, 600f).WithMessage("Czas rzutu musi mieścić się w czasie rundy.");
		});
	}

	#endregion
}
