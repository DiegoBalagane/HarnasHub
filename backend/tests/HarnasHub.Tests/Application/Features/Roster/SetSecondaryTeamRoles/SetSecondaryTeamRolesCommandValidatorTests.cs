using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Roster.SetSecondaryTeamRoles;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Roster.SetSecondaryTeamRoles;

public class SetSecondaryTeamRolesCommandValidatorTests
{
	#region Private Fields

	private readonly SetSecondaryTeamRolesCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_have_error_when_the_user_id_is_empty()
	{
		var result = _validator.TestValidate(new SetSecondaryTeamRolesCommand(Guid.Empty, [TeamRole.AWPer]));

		result.ShouldHaveValidationErrorFor(x => x.UserId);
	}

	[Fact]
	public void Should_have_error_for_duplicate_roles()
	{
		var result = _validator.TestValidate(
			new SetSecondaryTeamRolesCommand(Guid.NewGuid(), [TeamRole.AWPer, TeamRole.AWPer]));

		result.ShouldHaveValidationErrorFor(x => x.TeamRoles);
	}

	[Fact]
	public void Should_not_have_errors_for_an_empty_list()
	{
		var result = _validator.TestValidate(new SetSecondaryTeamRolesCommand(Guid.NewGuid(), []));

		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void Should_not_have_errors_for_distinct_valid_roles()
	{
		var result = _validator.TestValidate(
			new SetSecondaryTeamRolesCommand(Guid.NewGuid(), [TeamRole.AWPer, TeamRole.IGL]));

		result.ShouldNotHaveAnyValidationErrors();
	}

	#endregion
}
