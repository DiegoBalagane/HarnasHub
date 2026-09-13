using FluentValidation;

namespace HarnasHub.Application.Features.TrainingMaterials.AddMaterial;

/// <summary>Validation rules for <see cref="AddMaterialCommand"/>.</summary>
public class AddMaterialCommandValidator : AbstractValidator<AddMaterialCommand>
{
    #region Constructors

    public AddMaterialCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tytuł jest wymagany.")
            .MaximumLength(150).WithMessage("Tytuł może mieć maksymalnie 150 znaków.");

        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("Link jest wymagany.")
            .Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .WithMessage("Link musi być poprawnym adresem URL.");
    }

    #endregion
}
