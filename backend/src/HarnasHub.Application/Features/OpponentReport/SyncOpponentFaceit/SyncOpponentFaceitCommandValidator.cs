using FluentValidation;

namespace HarnasHub.Application.Features.OpponentReport.SyncOpponentFaceit;

/// <summary>Validation rules for <see cref="SyncOpponentFaceitCommand"/>.</summary>
public class SyncOpponentFaceitCommandValidator : AbstractValidator<SyncOpponentFaceitCommand>
{
	#region Constructors

	public SyncOpponentFaceitCommandValidator()
	{
		RuleFor(x => x.OpponentName)
			.NotEmpty().WithMessage("Nazwa przeciwnika jest wymagana.")
			.MaximumLength(100).WithMessage("Nazwa przeciwnika może mieć maksymalnie 100 znaków.");
	}

	#endregion
}
