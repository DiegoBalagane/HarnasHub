using FluentValidation;
using HarnasHub.Application.Features.OpponentReport.Shared;

namespace HarnasHub.Application.Features.OpponentReport.LinkOpponentFaceit;

/// <summary>Validation rules for <see cref="LinkOpponentFaceitCommand"/>.</summary>
public class LinkOpponentFaceitCommandValidator : AbstractValidator<LinkOpponentFaceitCommand>
{
	#region Constructors

	public LinkOpponentFaceitCommandValidator()
	{
		RuleFor(x => x.OpponentName)
			.NotEmpty().WithMessage("Nazwa przeciwnika jest wymagana.")
			.MaximumLength(100).WithMessage("Nazwa przeciwnika może mieć maksymalnie 100 znaków.");

		RuleFor(x => x.Source)
			.NotEmpty().WithMessage("Wklej link do drużyny FACEIT, link do pokoju meczowego albo listę nicków.")
			.MaximumLength(1000).WithMessage("Wklejony tekst może mieć maksymalnie 1000 znaków.")
			.Must(source => FaceitLinkParser.Parse(source) is not null)
			.WithMessage($"Nie rozpoznano linku FACEIT ani listy nicków (maksymalnie {FaceitLinkParser.MaxNicknames}, oddzielone przecinkami lub spacjami).");
	}

	#endregion
}
