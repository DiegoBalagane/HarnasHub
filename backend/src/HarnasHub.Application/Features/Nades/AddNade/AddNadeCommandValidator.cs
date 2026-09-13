using FluentValidation;

namespace HarnasHub.Application.Features.Nades.AddNade;

/// <summary>Validation rules for <see cref="AddNadeCommand"/>.</summary>
public class AddNadeCommandValidator : AbstractValidator<AddNadeCommand>
{
    #region Constructors

    public AddNadeCommandValidator()
    {
        RuleFor(x => x.MapName)
            .NotEmpty().WithMessage("Nazwa mapy jest wymagana.")
            .MaximumLength(50).WithMessage("Nazwa mapy może mieć maksymalnie 50 znaków.");

        RuleFor(x => x.Type).IsInEnum().WithMessage("Nieprawidłowy typ granatu.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tytuł pozycji jest wymagany.")
            .MaximumLength(100).WithMessage("Tytuł może mieć maksymalnie 100 znaków.");

        RuleFor(x => x.YoutubeUrl)
            .Must(url => string.IsNullOrWhiteSpace(url) || Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .WithMessage("Link do wideo musi być poprawnym adresem URL.");
    }

    #endregion
}
