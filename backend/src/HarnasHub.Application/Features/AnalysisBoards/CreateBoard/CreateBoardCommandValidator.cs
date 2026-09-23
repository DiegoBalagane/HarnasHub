using FluentValidation;

namespace HarnasHub.Application.Features.AnalysisBoards.CreateBoard;

/// <summary>Validation rules for <see cref="CreateBoardCommand"/>.</summary>
public class CreateBoardCommandValidator : AbstractValidator<CreateBoardCommand>
{
	#region Constructors

	public CreateBoardCommandValidator()
	{
		RuleFor(x => x.MapName).IsInEnum().WithMessage("Nieprawidłowa mapa.");

		RuleFor(x => x.Title)
			.NotEmpty().WithMessage("Tytuł tablicy jest wymagany.")
			.MaximumLength(150).WithMessage("Tytuł może mieć maksymalnie 150 znaków.");

		RuleFor(x => x.StrokesJson).NotEmpty().WithMessage("Brak danych rysunku.");
	}

	#endregion
}
