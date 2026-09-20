using FluentValidation;

namespace HarnasHub.Application.Features.Leagues.CreateLeague;

/// <summary>Validation rules for <see cref="CreateLeagueCommand"/>.</summary>
public class CreateLeagueCommandValidator : AbstractValidator<CreateLeagueCommand>
{
	#region Constructors

	public CreateLeagueCommandValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("Nazwa ligi jest wymagana.")
			.MaximumLength(100).WithMessage("Nazwa ligi może mieć maksymalnie 100 znaków.");

		RuleFor(x => x.Season)
			.NotEmpty().WithMessage("Sezon jest wymagany.")
			.MaximumLength(50).WithMessage("Sezon może mieć maksymalnie 50 znaków.");

		RuleFor(x => x.Type).IsInEnum().WithMessage("Nieprawidłowy typ ligi.");
	}

	#endregion
}
