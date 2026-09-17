using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Roster.SetIsCoach;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Roster.SetIsCoach;

public class SetIsCoachCommandValidatorTests
{
	#region Private Fields

	private readonly SetIsCoachCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_have_error_when_user_id_is_empty()
	{
		var command = new SetIsCoachCommand(Guid.Empty, true);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.UserId);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void Should_not_have_errors_for_either_toggle_value(bool isCoach)
	{
		var command = new SetIsCoachCommand(Guid.NewGuid(), isCoach);

		var result = _validator.TestValidate(command);

		result.ShouldNotHaveAnyValidationErrors();
	}

	#endregion
}
