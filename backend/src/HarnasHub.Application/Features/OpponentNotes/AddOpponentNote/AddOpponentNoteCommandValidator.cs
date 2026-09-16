using FluentValidation;

namespace HarnasHub.Application.Features.OpponentNotes.AddOpponentNote;

/// <summary>Validation rules for <see cref="AddOpponentNoteCommand"/>.</summary>
public class AddOpponentNoteCommandValidator : AbstractValidator<AddOpponentNoteCommand>
{
	#region Constructors

	public AddOpponentNoteCommandValidator()
	{
		RuleFor(x => x.OpponentName)
			.NotEmpty().WithMessage("Nazwa przeciwnika jest wymagana.")
			.MaximumLength(100).WithMessage("Nazwa przeciwnika może mieć maksymalnie 100 znaków.");

		RuleFor(x => x.Content).NotEmpty().WithMessage("Treść notatki jest wymagana.");

		RuleFor(x => x.MaterialUrl)
			.Must(url => string.IsNullOrWhiteSpace(url) || Uri.IsWellFormedUriString(url, UriKind.Absolute))
			.WithMessage("Link musi być poprawnym adresem URL.");
	}

	#endregion
}
