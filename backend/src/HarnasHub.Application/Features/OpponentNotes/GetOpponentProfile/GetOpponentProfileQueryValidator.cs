using FluentValidation;

namespace HarnasHub.Application.Features.OpponentNotes.GetOpponentProfile;

/// <summary>Validation rules for <see cref="GetOpponentProfileQuery"/>.</summary>
public class GetOpponentProfileQueryValidator : AbstractValidator<GetOpponentProfileQuery>
{
	#region Constructors

	public GetOpponentProfileQueryValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("Nazwa przeciwnika jest wymagana.")
			.MaximumLength(100).WithMessage("Nazwa przeciwnika może mieć maksymalnie 100 znaków.");
	}

	#endregion
}
