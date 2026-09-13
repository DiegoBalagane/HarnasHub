using FluentValidation;

namespace HarnasHub.Application.Features.Auth.Register;

/// <summary>Validation rules for <see cref="RegisterCommand"/>.</summary>
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    #region Constructors

    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail jest wymagany.")
            .EmailAddress().WithMessage("Podaj poprawny adres e-mail.");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Nazwa wyświetlana jest wymagana.")
            .MaximumLength(50).WithMessage("Nazwa wyświetlana może mieć maksymalnie 50 znaków.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Hasło jest wymagane.")
            .MinimumLength(8).WithMessage("Hasło musi mieć co najmniej 8 znaków.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Nieprawidłowa rola.");
    }

    #endregion
}
