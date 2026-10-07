using FluentValidation;

namespace HarnasHub.Application.Features.TeamInfo.CreateTeamInfoEntry;

/// <summary>Validation rules for <see cref="CreateTeamInfoEntryCommand"/>.</summary>
public class CreateTeamInfoEntryCommandValidator : AbstractValidator<CreateTeamInfoEntryCommand>
{
	#region Constructors

	public CreateTeamInfoEntryCommandValidator()
	{
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
