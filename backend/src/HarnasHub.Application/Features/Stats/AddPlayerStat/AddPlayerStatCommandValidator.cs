using FluentValidation;

namespace HarnasHub.Application.Features.Stats.AddPlayerStat;

/// <summary>Validation rules for <see cref="AddPlayerStatCommand"/>.</summary>
public class AddPlayerStatCommandValidator : AbstractValidator<AddPlayerStatCommand>
{
	#region Constructors

	public AddPlayerStatCommandValidator()
	{
		RuleFor(x => x.MatchResultId).NotEmpty().WithMessage("Nieprawidłowy mecz.");
		RuleFor(x => x.UserId).NotEmpty().WithMessage("Wybierz zawodnika.");
		RuleFor(x => x.Kills).GreaterThanOrEqualTo(0).WithMessage("Liczba zabójstw nie może być ujemna.");
		RuleFor(x => x.Deaths).GreaterThanOrEqualTo(0).WithMessage("Liczba śmierci nie może być ujemna.");
		RuleFor(x => x.Assists).GreaterThanOrEqualTo(0).WithMessage("Liczba asyst nie może być ujemna.");
		RuleFor(x => x.Adr).GreaterThanOrEqualTo(0).WithMessage("ADR nie może być ujemne.");
		RuleFor(x => x.HeadshotPercentage).InclusiveBetween(0, 100).WithMessage("HS% musi być między 0 a 100.");
		RuleFor(x => x.Rating).GreaterThanOrEqualTo(0).WithMessage("Rating nie może być ujemny.");

		RuleFor(x => x.EntryKills).GreaterThanOrEqualTo(0).When(x => x.EntryKills.HasValue, ApplyConditionTo.CurrentValidator)
			.WithMessage("Liczba entry killi nie może być ujemna.");
		RuleFor(x => x.EntryDeaths).GreaterThanOrEqualTo(0).When(x => x.EntryDeaths.HasValue, ApplyConditionTo.CurrentValidator)
			.WithMessage("Liczba entry śmierci nie może być ujemna.");
		RuleFor(x => x.KastPercentage).InclusiveBetween(0, 100).When(x => x.KastPercentage.HasValue, ApplyConditionTo.CurrentValidator)
			.WithMessage("KAST% musi być między 0 a 100.");
		RuleFor(x => x.UtilityDamage).GreaterThanOrEqualTo(0).When(x => x.UtilityDamage.HasValue, ApplyConditionTo.CurrentValidator)
			.WithMessage("Obrażenia z granatów nie mogą być ujemne.");
		RuleFor(x => x.FlashAssists).GreaterThanOrEqualTo(0).When(x => x.FlashAssists.HasValue, ApplyConditionTo.CurrentValidator)
			.WithMessage("Liczba asyst z flashy nie może być ujemna.");
	}

	#endregion
}
