using FluentValidation;

namespace HarnasHub.Application.Features.AnalysisBoards.UpdateBoard;

/// <summary>Validation rules for <see cref="UpdateBoardCommand"/>.</summary>
public class UpdateBoardCommandValidator : AbstractValidator<UpdateBoardCommand>
{
	#region Constructors

	public UpdateBoardCommandValidator()
	{
		RuleFor(x => x.Title)
			.NotEmpty().WithMessage("Tytuł tablicy jest wymagany.")
			.MaximumLength(150).WithMessage("Tytuł może mieć maksymalnie 150 znaków.");

		RuleFor(x => x.StrokesJson).NotEmpty().WithMessage("Brak danych rysunku.");
	}

	#endregion
}
