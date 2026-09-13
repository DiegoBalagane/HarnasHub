using FluentValidation;

namespace HarnasHub.Application.Features.Roster.UpdateUserRole;

/// <summary>Validation rules for <see cref="UpdateUserRoleCommand"/>.</summary>
public class UpdateUserRoleCommandValidator : AbstractValidator<UpdateUserRoleCommand>
{
    #region Constructors

    public UpdateUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Nieprawidłowy zawodnik.");
        RuleFor(x => x.Role).IsInEnum().WithMessage("Nieprawidłowa rola.");
    }

    #endregion
}
