using FluentValidation;

namespace HarnasHub.Application.Features.Tasks.UpdateTask;

/// <summary>Validation rules for <see cref="UpdateTaskCommand"/>.</summary>
public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
	#region Constructors

	public UpdateTaskCommandValidator()
	{
		RuleFor(x => x.Title)
			.NotEmpty().WithMessage("Tytuł zadania jest wymagany.")
			.MaximumLength(150).WithMessage("Tytuł może mieć maksymalnie 150 znaków.");
	}

	#endregion
}
