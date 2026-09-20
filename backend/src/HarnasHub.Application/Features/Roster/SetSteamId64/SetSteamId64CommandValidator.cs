using FluentValidation;

namespace HarnasHub.Application.Features.Roster.SetSteamId64;

/// <summary>Validation rules for <see cref="SetSteamId64Command"/> — identical to the self-service version's format check.</summary>
public class SetSteamId64CommandValidator : AbstractValidator<SetSteamId64Command>
{
	#region Constructors

	public SetSteamId64CommandValidator()
	{
		// A null/blank SteamID64 is a deliberate "clear" request, not a validation failure — only a set value has to look real.
		When(x => !string.IsNullOrWhiteSpace(x.SteamId64), () =>
		{
			RuleFor(x => x.SteamId64)
				.Must(value => value!.Trim().Length == 17 && value.Trim().All(char.IsAsciiDigit))
				.WithMessage("SteamID64 to 17-cyfrowy numer (np. 76561198012345678).")
				// A valid Steam individual account64 starts at 76561197960265728 — anything before that,
				// even if 17 digits, is not a real account.
				.Must(value => ulong.TryParse(value!.Trim(), out var parsed) && parsed >= 76561197960265728)
				.WithMessage("To nie wygląda na prawidłowy SteamID64.");
		});
	}

	#endregion
}
