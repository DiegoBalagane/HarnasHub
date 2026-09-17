using FluentValidation;

namespace HarnasHub.Application.Features.Roster.DeleteTeamMember;

/// <summary>Validation rules for <see cref="DeleteTeamMemberCommand"/>.</summary>
public class DeleteTeamMemberCommandValidator : AbstractValidator<DeleteTeamMemberCommand>
{
	#region Constructors

	public DeleteTeamMemberCommandValidator()
	{
		RuleFor(x => x.UserId).NotEmpty().WithMessage("Nieprawidłowy zawodnik.");
	}

	#endregion
}
