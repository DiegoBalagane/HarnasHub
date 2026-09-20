using FluentValidation;
using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Features.Results.AddResult;

/// <summary>Validation rules for <see cref="AddResultCommand"/>.</summary>
public class AddResultCommandValidator : AbstractValidator<AddResultCommand>
{
	#region Constructors

	public AddResultCommandValidator()
	{
		RuleFor(x => x.Opponent)
			.NotEmpty().WithMessage("Nazwa przeciwnika jest wymagana.")
			.MaximumLength(100).WithMessage("Nazwa przeciwnika może mieć maksymalnie 100 znaków.");

		RuleFor(x => x.OurScore).GreaterThanOrEqualTo(0).WithMessage("Wynik nie może być ujemny.");
		RuleFor(x => x.OpponentScore).GreaterThanOrEqualTo(0).WithMessage("Wynik nie może być ujemny.");

		RuleFor(x => x.DemoUrl)
			.Must(url => string.IsNullOrWhiteSpace(url) || Uri.IsWellFormedUriString(url, UriKind.Absolute))
			.WithMessage("Link do demki musi być poprawnym adresem URL.");

		RuleFor(x => x.Category).IsInEnum().WithMessage("Nieprawidłowa kategoria rozgrywki.");

		// .When() defaults to applying retroactively to every validator already in the chain (ApplyConditionTo.AllValidators),
		// so without CurrentValidator here the second .When() would silently override the first one's condition too.
		RuleFor(x => x.TournamentId)
			.NotNull().WithMessage("Wybierz turniej.").When(x => x.Category == MatchCategory.Tournament, ApplyConditionTo.CurrentValidator)
			.Null().WithMessage("Turniej można wybrać tylko dla kategorii Turniej.").When(x => x.Category != MatchCategory.Tournament, ApplyConditionTo.CurrentValidator);

		RuleFor(x => x.LeagueId)
			.NotNull().WithMessage("Wybierz ligę.").When(x => x.Category == MatchCategory.League, ApplyConditionTo.CurrentValidator)
			.Null().WithMessage("Ligę można wybrać tylko dla kategorii Liga.").When(x => x.Category != MatchCategory.League, ApplyConditionTo.CurrentValidator);
	}

	#endregion
}
