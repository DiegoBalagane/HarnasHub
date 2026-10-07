using FluentValidation;

namespace HarnasHub.Application.Features.TeamInfo.UpdateTeamInfoEntry;

/// <summary>Validation rules for <see cref="UpdateTeamInfoEntryCommand"/>.</summary>
public class UpdateTeamInfoEntryCommandValidator : AbstractValidator<UpdateTeamInfoEntryCommand>
{
	#region Constructors

	public UpdateTeamInfoEntryCommandValidator()
	{
		RuleFor(x => x.Id).NotEmpty().WithMessage("Identyfikator wpisu jest wymagany.");

		RuleFor(x => x.Category)
			.NotEmpty().WithMessage("Kategoria jest wymagana.")
			.MaximumLength(50).WithMessage("Kategoria może mieć maksymalnie 50 znaków.");

		RuleFor(x => x.Title)
			.NotEmpty().WithMessage("Tytuł jest wymagany.")
			.MaximumLength(100).WithMessage("Tytuł może mieć maksymalnie 100 znaków.");

		RuleFor(x => x.Value)
			.NotEmpty().WithMessage("Wartość jest wymagana.")
			.MaximumLength(2000).WithMessage("Wartość może mieć maksymalnie 2000 znaków.");
	}

	#endregion
}
