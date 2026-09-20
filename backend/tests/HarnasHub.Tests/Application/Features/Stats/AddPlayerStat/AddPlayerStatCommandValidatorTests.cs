using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Stats.AddPlayerStat;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Stats.AddPlayerStat;

public class AddPlayerStatCommandValidatorTests
{
	#region Private Fields

	private readonly AddPlayerStatCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_have_error_when_headshot_percentage_is_out_of_range()
	{
		var command = new AddPlayerStatCommand(Guid.NewGuid(), Guid.NewGuid(), 20, 15, 5, 75.5, 150, 1.2);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.HeadshotPercentage);
	}

	[Fact]
	public void Should_have_error_when_kills_is_negative()
	{
		var command = new AddPlayerStatCommand(Guid.NewGuid(), Guid.NewGuid(), -1, 15, 5, 75.5, 50, 1.2);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.Kills);
	}

	[Fact]
	public void Should_not_have_errors_for_a_valid_command()
	{
		var command = new AddPlayerStatCommand(Guid.NewGuid(), Guid.NewGuid(), 20, 15, 5, 75.5, 50, 1.2);

		var result = _validator.TestValidate(command);

		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void Should_have_error_when_kast_percentage_is_out_of_range()
	{
		var command = new AddPlayerStatCommand(
			Guid.NewGuid(), Guid.NewGuid(), 20, 15, 5, 75.5, 50, 1.2, KastPercentage: 120);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.KastPercentage);
	}

	[Fact]
	public void Should_have_error_when_entry_kills_is_negative()
	{
		var command = new AddPlayerStatCommand(
			Guid.NewGuid(), Guid.NewGuid(), 20, 15, 5, 75.5, 50, 1.2, EntryKills: -1);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.EntryKills);
	}

	[Fact]
	public void Should_not_have_errors_for_a_full_demo_derived_command()
	{
		var command = new AddPlayerStatCommand(
			Guid.NewGuid(), Guid.NewGuid(), 20, 15, 5, 75.5, 50, 1.2,
			EntryKills: 4, EntryDeaths: 2, KastPercentage: 75, MultiKill2K: 3, MultiKill3K: 1,
			MultiKill4K: 0, MultiKill5K: 0, UtilityDamage: 120, FlashAssists: 2);

		var result = _validator.TestValidate(command);

		result.ShouldNotHaveAnyValidationErrors();
	}

	#endregion
}
