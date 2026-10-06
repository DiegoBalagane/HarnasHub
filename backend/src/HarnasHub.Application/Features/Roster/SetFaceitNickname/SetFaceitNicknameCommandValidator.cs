using FluentValidation;

namespace HarnasHub.Application.Features.Roster.SetFaceitNickname;

/// <summary>Validation rules for <see cref="SetFaceitNicknameCommand"/>.</summary>
public class SetFaceitNicknameCommandValidator : AbstractValidator<SetFaceitNicknameCommand>
{
	#region Public Fields

	/// <summary>Longest FACEIT nickname accepted (matches the column length).</summary>
	public const int MaxLength = 64;

	#endregion

	#region Constructors

	public SetFaceitNicknameCommandValidator()
	{
		RuleFor(x => x.UserId).NotEmpty().WithMessage("Nieprawidłowy zawodnik.");

		// A null value is a deliberate "clear" request; a set value must not be blank or too long.
		When(x => x.Nickname is not null, () =>
		{
			RuleFor(x => x.Nickname!)
				.Must(value => !string.IsNullOrWhiteSpace(value))
				.WithMessage("Nick FACEIT nie może być pusty.")
				.Must(value => value.Trim().Length <= MaxLength)
				.WithMessage($"Nick FACEIT może mieć maksymalnie {MaxLength} znaków.");
		});
	}

	#endregion
}
