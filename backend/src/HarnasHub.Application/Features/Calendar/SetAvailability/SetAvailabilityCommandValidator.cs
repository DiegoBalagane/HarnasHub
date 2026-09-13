using FluentValidation;

namespace HarnasHub.Application.Features.Calendar.SetAvailability;

/// <summary>Validation rules for <see cref="SetAvailabilityCommand"/>.</summary>
public class SetAvailabilityCommandValidator : AbstractValidator<SetAvailabilityCommand>
{
    #region Constructors

    public SetAvailabilityCommandValidator()
    {
        RuleFor(x => x.EventId).NotEmpty().WithMessage("Nieprawidłowe wydarzenie.");
        RuleFor(x => x.Status).IsInEnum().WithMessage("Nieprawidłowy status dostępności.");
    }

    #endregion
}
