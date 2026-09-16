using FluentValidation;

namespace HarnasHub.Application.Features.Roster.UpdateOwnNickname;

/// <summary>Validation rules for <see cref="UpdateOwnNicknameCommand"/>.</summary>
public class UpdateOwnNicknameCommandValidator : AbstractValidator<UpdateOwnNicknameCommand>
{
	#region Private Fields

	private const int MinNicknameLength = 2;
	private const int MaxNicknameLength = 32;

	#endregion

	#region Constructors

	public UpdateOwnNicknameCommandValidator()
	{
		RuleFor(x => x.Nickname)
			.NotEmpty().WithMessage("Podaj nick.")
			.Must(nickname => (nickname ?? string.Empty).Trim().Length >= MinNicknameLength)
				.WithMessage($"Nick musi mieć co najmniej {MinNicknameLength} znaki.")
			.Must(nickname => (nickname ?? string.Empty).Trim().Length <= MaxNicknameLength)
				.WithMessage($"Nick może mieć maksymalnie {MaxNicknameLength} znaki.");
	}

	#endregion
}
