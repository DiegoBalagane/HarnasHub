using FluentValidation;

namespace HarnasHub.Application.Features.Calendar.UpdateEvent;

/// <summary>Validation rules for <see cref="UpdateEventCommand"/>, mirroring <see cref="CreateEvent.CreateEventCommandValidator"/>.</summary>
public class UpdateEventCommandValidator : AbstractValidator<UpdateEventCommand>
{
	#region Constructors

	public UpdateEventCommandValidator()
	{
		RuleFor(x => x.EventId).NotEmpty().WithMessage("Nieprawidłowy identyfikator wydarzenia.");

		RuleFor(x => x.Title)
			.NotEmpty().WithMessage("Tytuł wydarzenia jest wymagany.")
			.MaximumLength(100).WithMessage("Tytuł może mieć maksymalnie 100 znaków.");

		RuleFor(x => x.Type).IsInEnum().WithMessage("Nieprawidłowy typ wydarzenia.");

		RuleFor(x => x.Location).MaximumLength(200).WithMessage("Lokalizacja może mieć maksymalnie 200 znaków.");

		RuleFor(x => x.Url)
			.Must(url => string.IsNullOrWhiteSpace(url) || Uri.IsWellFormedUriString(url, UriKind.Absolute))
			.WithMessage("Link musi być poprawnym adresem URL.");
	}

	#endregion
}
