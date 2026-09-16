using FluentValidation;

namespace HarnasHub.Application.Features.Tasks.AssignTask;

/// <summary>Validation rules for <see cref="AssignTaskCommand"/>.</summary>
public class AssignTaskCommandValidator : AbstractValidator<AssignTaskCommand>
{
	#region Constructors

	public AssignTaskCommandValidator()
	{
		RuleFor(x => x.Title)
			.NotEmpty().WithMessage("Tytuł zadania jest wymagany.")
			.MaximumLength(150).WithMessage("Tytuł może mieć maksymalnie 150 znaków.");

		RuleFor(x => x.AssignedToUserId).NotEmpty().WithMessage("Wybierz zawodnika.");
	}

	#endregion
}
