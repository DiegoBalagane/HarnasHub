using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Roster.UpdateAccessLevel;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Roster.UpdateAccessLevel;

public class UpdateAccessLevelCommandValidatorTests
{
	#region Private Fields

	private readonly UpdateAccessLevelCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_have_error_when_user_id_is_empty()
	{
		var command = new UpdateAccessLevelCommand(Guid.Empty, AccessLevel.Player);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.UserId);
	}

	[Theory]
	[InlineData(AccessLevel.Guest)]
	[InlineData(AccessLevel.Player)]
	[InlineData(AccessLevel.Manager)]
	public void Should_not_have_errors_for_any_known_access_level(AccessLevel accessLevel)
	{
		var command = new UpdateAccessLevelCommand(Guid.NewGuid(), accessLevel);

		var result = _validator.TestValidate(command);

		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void Should_have_error_when_access_level_is_outside_the_enum()
	{
		var command = new UpdateAccessLevelCommand(Guid.NewGuid(), (AccessLevel)99);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.AccessLevel);
	}

	#endregion
}
