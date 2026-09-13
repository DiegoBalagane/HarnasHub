using FluentValidation;

namespace HarnasHub.Application.Features.Results.AddResult;

/// <summary>Validation rules for <see cref="AddResultCommand"/>.</summary>
public class AddResultCommandValidator : AbstractValidator<AddResultCommand>
{
    #region Constructors

    public AddResultCommandValidator()
    {
        RuleFor(x => x.Opponent)
            .NotEmpty().WithMessage("Nazwa przeciwnika jest wymagana.")
            .MaximumLength(100).WithMessage("Nazwa przeciwnika może mieć maksymalnie 100 znaków.");

        RuleFor(x => x.OurScore).GreaterThanOrEqualTo(0).WithMessage("Wynik nie może być ujemny.");
        RuleFor(x => x.OpponentScore).GreaterThanOrEqualTo(0).WithMessage("Wynik nie może być ujemny.");

        RuleFor(x => x.DemoUrl)
            .Must(url => string.IsNullOrWhiteSpace(url) || Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .WithMessage("Link do demki musi być poprawnym adresem URL.");
    }

    #endregion
}
