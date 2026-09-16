using FluentValidation;

namespace HarnasHub.Application.Features.Auth.DiscordLogin;

/// <summary>Validation rules for <see cref="DiscordLoginCommand"/>.</summary>
public class DiscordLoginCommandValidator : AbstractValidator<DiscordLoginCommand>
{
	#region Constructors

	public DiscordLoginCommandValidator()
	{
		RuleFor(x => x.Code).NotEmpty().WithMessage("Brak kodu autoryzacyjnego z Discorda.");
	}

	#endregion
}
