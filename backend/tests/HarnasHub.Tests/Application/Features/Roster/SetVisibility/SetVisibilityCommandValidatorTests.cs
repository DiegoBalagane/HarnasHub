using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Roster.SetVisibility;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Roster.SetVisibility;

public class SetVisibilityCommandValidatorTests
{
	#region Public Methods

	[Fact]
	public void Should_accept_any_flag_combination_for_a_user()
	{
		var result = new SetVisibilityCommandValidator().TestValidate(new SetVisibilityCommand(Guid.NewGuid(), false, false));

		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void Should_reject_an_empty_user_id()
	{
		var result = new SetVisibilityCommandValidator().TestValidate(new SetVisibilityCommand(Guid.Empty, true, true));

		result.ShouldHaveValidationErrorFor(x => x.UserId);
	}

	#endregion
}
