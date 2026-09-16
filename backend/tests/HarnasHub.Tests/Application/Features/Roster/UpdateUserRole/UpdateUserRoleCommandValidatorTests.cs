using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Roster.UpdateUserRole;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Roster.UpdateUserRole;

public class UpdateUserRoleCommandValidatorTests
{
	#region Private Fields

	private readonly UpdateUserRoleCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_have_error_when_user_id_is_empty()
	{
		var command = new UpdateUserRoleCommand(Guid.Empty, UserRole.Coach);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.UserId);
	}

	[Theory]
	[InlineData(UserRole.Guest)]
	[InlineData(UserRole.Player)]
	[InlineData(UserRole.Coach)]
	[InlineData(UserRole.Manager)]
	public void Should_not_have_errors_for_any_known_role(UserRole role)
	{
		var command = new UpdateUserRoleCommand(Guid.NewGuid(), role);

		var result = _validator.TestValidate(command);

		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void Should_have_error_when_role_is_outside_the_enum()
	{
		var command = new UpdateUserRoleCommand(Guid.NewGuid(), (UserRole)99);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.Role);
	}

	#endregion
}
