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
		// A null/blank nickname is a deliberate "clear back to Discord name" request, not a validation
		// failure — only a non-blank value has to meet the length bounds.
		When(x => !string.IsNullOrWhiteSpace(x.Nickname), () =>
		{
			RuleFor(x => x.Nickname)
				.Must(nickname => nickname!.Trim().Length >= MinNicknameLength)
					.WithMessage($"Nick musi mieć co najmniej {MinNicknameLength} znaki.")
				.Must(nickname => nickname!.Trim().Length <= MaxNicknameLength)
					.WithMessage($"Nick może mieć maksymalnie {MaxNicknameLength} znaki.");
		});
	}

	#endregion
}
