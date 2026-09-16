using FluentValidation;

namespace HarnasHub.Application.Features.Calendar.CreateEvent;

/// <summary>Validation rules for <see cref="CreateEventCommand"/>.</summary>
public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
	#region Constructors

	public CreateEventCommandValidator()
	{
		RuleFor(x => x.Title)
			.NotEmpty().WithMessage("Tytuł wydarzenia jest wymagany.")
			.MaximumLength(100).WithMessage("Tytuł może mieć maksymalnie 100 znaków.");

		RuleFor(x => x.Type).IsInEnum().WithMessage("Nieprawidłowy typ wydarzenia.");

		RuleFor(x => x.Location).MaximumLength(200).WithMessage("Lokalizacja może mieć maksymalnie 200 znaków.");
	}

	#endregion
}
