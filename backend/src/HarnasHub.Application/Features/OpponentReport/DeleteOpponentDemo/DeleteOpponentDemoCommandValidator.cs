#region Usings

using FluentValidation;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.DeleteOpponentDemo;

/// <summary>Validation rules for <see cref="DeleteOpponentDemoCommand"/>.</summary>
public class DeleteOpponentDemoCommandValidator : AbstractValidator<DeleteOpponentDemoCommand>
{
	#region Constructors

	/// <summary>Requires the demo id.</summary>
	public DeleteOpponentDemoCommandValidator()
	{
		RuleFor(x => x.Id).NotEmpty().WithMessage("Brak identyfikatora demki.");
	}

	#endregion
}
