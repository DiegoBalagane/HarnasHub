using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Roster.UpdateOwnPinColor;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Roster.UpdateOwnPinColor;

public class UpdateOwnPinColorCommandValidatorTests
{
	#region Private Fields

	private readonly UpdateOwnPinColorCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_not_have_errors_when_clearing_the_color()
	{
		var result = _validator.TestValidate(new UpdateOwnPinColorCommand(null));

		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void Should_not_have_errors_for_a_valid_color()
	{
		var result = _validator.TestValidate(new UpdateOwnPinColorCommand(PinColor.Green));

		result.ShouldNotHaveAnyValidationErrors();
	}

	#endregion
}
