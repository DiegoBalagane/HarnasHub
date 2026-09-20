using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Roster.SetSteamId64;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Roster.SetSteamId64;

public class SetSteamId64CommandValidatorTests
{
	#region Private Fields

	private readonly SetSteamId64CommandValidator _validator = new();
	private readonly Guid _userId = Guid.NewGuid();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_accept_a_null_value_as_clearing_it()
	{
		var result = _validator.TestValidate(new SetSteamId64Command(_userId, null));

		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void Should_accept_a_realistic_steam_id_64()
	{
		var result = _validator.TestValidate(new SetSteamId64Command(_userId, "76561198012345678"));

		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void Should_reject_a_value_with_the_wrong_number_of_digits()
	{
		var result = _validator.TestValidate(new SetSteamId64Command(_userId, "123456"));

		result.ShouldHaveValidationErrorFor(x => x.SteamId64);
	}

	#endregion
}
