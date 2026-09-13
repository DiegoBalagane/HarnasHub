using FluentValidation;

namespace HarnasHub.Application.Features.Auth.Login;

/// <summary>Validation rules for <see cref="LoginQuery"/>.</summary>
public class LoginQueryValidator : AbstractValidator<LoginQuery>
{
    #region Constructors

    public LoginQueryValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Podaj poprawny adres e-mail.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Hasło jest wymagane.");
    }

    #endregion
}
