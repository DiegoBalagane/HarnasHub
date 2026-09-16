using FluentValidation;

namespace HarnasHub.Application.Features.Roster.UpdateTeamRole;

/// <summary>Validation rules for <see cref="UpdateTeamRoleCommand"/>.</summary>
public class UpdateTeamRoleCommandValidator : AbstractValidator<UpdateTeamRoleCommand>
{
	#region Constructors

	public UpdateTeamRoleCommandValidator()
	{
		RuleFor(x => x.UserId).NotEmpty().WithMessage("Nieprawidłowy zawodnik.");

		RuleFor(x => x.NewTeamRole!.Value)
			.IsInEnum()
			.WithMessage("Nieprawidłowa rola w drużynie.")
			.When(x => x.NewTeamRole.HasValue);
	}

	#endregion
}
