using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Roster.SetPinColor;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Roster.SetPinColor;

public class SetPinColorCommandValidatorTests
{
	#region Private Fields

	private readonly SetPinColorCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_have_error_when_user_id_is_empty()
	{
		var result = _validator.TestValidate(new SetPinColorCommand(Guid.Empty, PinColor.Blue));

		result.ShouldHaveValidationErrorFor(x => x.UserId);
	}

	[Fact]
	public void Should_have_error_when_pin_color_is_outside_the_enum()
	{
		var result = _validator.TestValidate(new SetPinColorCommand(Guid.NewGuid(), (PinColor)99));

		Assert.False(result.IsValid);
	}

	[Fact]
	public void Should_not_have_errors_when_clearing_the_pin_color()
	{
		var result = _validator.TestValidate(new SetPinColorCommand(Guid.NewGuid(), null));

		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void Should_not_have_errors_for_a_valid_command()
	{
		var result = _validator.TestValidate(new SetPinColorCommand(Guid.NewGuid(), PinColor.Purple));

		result.ShouldNotHaveAnyValidationErrors();
	}

	#endregion
}
