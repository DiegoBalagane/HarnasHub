using FluentValidation;

namespace HarnasHub.Application.Features.Tournaments.CreateTournament;

/// <summary>Validation rules for <see cref="CreateTournamentCommand"/>.</summary>
public class CreateTournamentCommandValidator : AbstractValidator<CreateTournamentCommand>
{
	#region Constructors

	public CreateTournamentCommandValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("Nazwa turnieju jest wymagana.")
			.MaximumLength(100).WithMessage("Nazwa turnieju może mieć maksymalnie 100 znaków.");
	}

	#endregion
}
