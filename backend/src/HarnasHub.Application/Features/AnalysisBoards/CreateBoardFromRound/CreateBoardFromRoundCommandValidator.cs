#region Usings

using FluentValidation;

#endregion

namespace HarnasHub.Application.Features.AnalysisBoards.CreateBoardFromRound;

/// <summary>Validation rules for <see cref="CreateBoardFromRoundCommand"/>.</summary>
public class CreateBoardFromRoundCommandValidator : AbstractValidator<CreateBoardFromRoundCommand>
{
	#region Constructors

	/// <summary>Defines the rules.</summary>
	public CreateBoardFromRoundCommandValidator()
	{
		RuleFor(x => x.Source).IsInEnum().WithMessage("Nieprawidłowe źródło rundy.");
		RuleFor(x => x.SourceId).NotEmpty().WithMessage("Brak identyfikatora meczu lub demki.");
		RuleFor(x => x.RoundNumber).GreaterThan(0).WithMessage("Numer rundy musi być dodatni.");
		RuleFor(x => x.Second).InclusiveBetween(0, 600).WithMessage("Sekunda rundy musi mieścić się w zakresie 0–600.");
	}

	#endregion
}
