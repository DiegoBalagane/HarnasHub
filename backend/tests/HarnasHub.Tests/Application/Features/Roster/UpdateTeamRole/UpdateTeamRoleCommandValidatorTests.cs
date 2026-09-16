using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Roster.UpdateTeamRole;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Roster.UpdateTeamRole;

public class UpdateTeamRoleCommandValidatorTests
{
	#region Private Fields

	private readonly UpdateTeamRoleCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_have_error_when_user_id_is_empty()
	{
		var result = _validator.TestValidate(new UpdateTeamRoleCommand(Guid.Empty, TeamRole.IGL));

		result.ShouldHaveValidationErrorFor(x => x.UserId);
	}

	[Fact]
	public void Should_have_error_when_team_role_is_outside_the_enum()
	{
		var result = _validator.TestValidate(new UpdateTeamRoleCommand(Guid.NewGuid(), (TeamRole)99));

		Assert.False(result.IsValid);
	}

	[Fact]
	public void Should_not_have_errors_when_clearing_the_team_role()
	{
		var result = _validator.TestValidate(new UpdateTeamRoleCommand(Guid.NewGuid(), null));

		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void Should_not_have_errors_for_a_valid_command()
	{
		var result = _validator.TestValidate(new UpdateTeamRoleCommand(Guid.NewGuid(), TeamRole.Lurker));

		result.ShouldNotHaveAnyValidationErrors();
	}

	#endregion
}
