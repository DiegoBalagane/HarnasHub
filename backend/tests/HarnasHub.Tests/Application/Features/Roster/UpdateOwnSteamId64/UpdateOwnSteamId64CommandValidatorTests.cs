using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Roster.UpdateOwnSteamId64;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Roster.UpdateOwnSteamId64;

public class UpdateOwnSteamId64CommandValidatorTests
{
	#region Private Fields

	private readonly UpdateOwnSteamId64CommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_accept_a_null_value_as_clearing_it()
	{
		var result = _validator.TestValidate(new UpdateOwnSteamId64Command(null));

		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void Should_accept_a_realistic_steam_id_64()
	{
		var result = _validator.TestValidate(new UpdateOwnSteamId64Command("76561198012345678"));

		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void Should_reject_a_value_below_the_lowest_possible_steam_id_64()
	{
		// 17 digits, but numerically below the lowest ever-issued individual account64.
		var result = _validator.TestValidate(new UpdateOwnSteamId64Command("00000000000123456"));

		result.ShouldHaveValidationErrorFor(x => x.SteamId64);
	}

	[Fact]
	public void Should_reject_a_value_with_the_wrong_number_of_digits()
	{
		var result = _validator.TestValidate(new UpdateOwnSteamId64Command("123456"));

		result.ShouldHaveValidationErrorFor(x => x.SteamId64);
	}

	[Fact]
	public void Should_reject_a_value_containing_non_digit_characters()
	{
		var result = _validator.TestValidate(new UpdateOwnSteamId64Command("STEAM_0:1:12345678"));

		result.ShouldHaveValidationErrorFor(x => x.SteamId64);
	}

	#endregion
}
