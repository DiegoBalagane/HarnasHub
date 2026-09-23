using FluentValidation;

namespace HarnasHub.Application.Features.Attendance.AddIncident;

/// <summary>Validation rules for <see cref="AddIncidentCommand"/>.</summary>
public class AddIncidentCommandValidator : AbstractValidator<AddIncidentCommand>
{
	#region Constructors

	public AddIncidentCommandValidator()
	{
		RuleFor(x => x.UserId).NotEmpty().WithMessage("Wybierz zawodnika.");

		RuleFor(x => x.OccurredOn).NotEqual(default(DateOnly)).WithMessage("Nieprawidłowa data.");

		RuleFor(x => x.Note).MaximumLength(300).WithMessage("Notatka może mieć maksymalnie 300 znaków.");
	}

	#endregion
}
