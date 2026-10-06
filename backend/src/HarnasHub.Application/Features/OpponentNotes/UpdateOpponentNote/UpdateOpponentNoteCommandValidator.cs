using FluentValidation;

namespace HarnasHub.Application.Features.OpponentNotes.UpdateOpponentNote;

/// <summary>Validation rules for <see cref="UpdateOpponentNoteCommand"/>, mirroring <see cref="AddOpponentNote.AddOpponentNoteCommandValidator"/>.</summary>
public class UpdateOpponentNoteCommandValidator : AbstractValidator<UpdateOpponentNoteCommand>
{
	#region Constructors

	public UpdateOpponentNoteCommandValidator()
	{
		RuleFor(x => x.NoteId).NotEmpty().WithMessage("Nieprawidłowy identyfikator notatki.");

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
