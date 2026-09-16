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
	}

	#endregion
}
