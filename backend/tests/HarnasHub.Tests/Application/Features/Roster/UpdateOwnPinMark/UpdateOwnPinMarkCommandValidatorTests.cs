using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Roster.UpdateOwnPinMark;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Roster.UpdateOwnPinMark;

public class UpdateOwnPinMarkCommandValidatorTests
{
	#region Private Fields

	private readonly UpdateOwnPinMarkCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_not_have_errors_when_clearing_the_mark()
	{
		var result = _validator.TestValidate(new UpdateOwnPinMarkCommand(null));

		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void Should_not_have_errors_for_a_single_letter()
	{
		var result = _validator.TestValidate(new UpdateOwnPinMarkCommand("7"));

		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void Should_not_have_errors_for_a_single_emoji_spanning_a_surrogate_pair()
	{
		var result = _validator.TestValidate(new UpdateOwnPinMarkCommand("🔥"));

		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void Should_have_an_error_for_more_than_one_character()
	{
		var result = _validator.TestValidate(new UpdateOwnPinMarkCommand("AB"));

		result.ShouldHaveValidationErrorFor(x => x.NewPinMark);
	}

	#endregion
}
