using FluentValidation;

namespace HarnasHub.Application.Features.OpponentManagement.GetOpponentDeletePreview;

/// <summary>Validation rules for <see cref="GetOpponentDeletePreviewQuery"/>.</summary>
public class GetOpponentDeletePreviewQueryValidator : AbstractValidator<GetOpponentDeletePreviewQuery>
{
	#region Constructors

	public GetOpponentDeletePreviewQueryValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("Nazwa przeciwnika jest wymagana.")
			.MaximumLength(100).WithMessage("Nazwa przeciwnika może mieć maksymalnie 100 znaków.");
	}

	#endregion
}
