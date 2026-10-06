#region Usings

using FluentValidation;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.DownloadOpponentDemos;

/// <summary>Validation rules for <see cref="DownloadOpponentDemosCommand"/>.</summary>
public class DownloadOpponentDemosCommandValidator : AbstractValidator<DownloadOpponentDemosCommand>
{
	#region Constructors

	/// <summary>Requires the opponent name and a demo count between 1 and <see cref="DownloadOpponentDemosCommand.MaxCount"/>.</summary>
	public DownloadOpponentDemosCommandValidator()
	{
		RuleFor(x => x.OpponentName)
			.NotEmpty().WithMessage("Nazwa przeciwnika jest wymagana.")
			.MaximumLength(100).WithMessage("Nazwa przeciwnika może mieć maksymalnie 100 znaków.");

		RuleFor(x => x.Count)
			.InclusiveBetween(1, DownloadOpponentDemosCommand.MaxCount)
			.WithMessage($"Można pobrać od 1 do {DownloadOpponentDemosCommand.MaxCount} demek naraz.");

		RuleForEach(x => x.Maps).IsInEnum().WithMessage("Nieznana mapa.");
	}

	#endregion
}
