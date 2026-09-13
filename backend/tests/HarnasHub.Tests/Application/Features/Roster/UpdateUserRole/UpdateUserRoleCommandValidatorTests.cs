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

    [Fact]
    public void Should_not_have_errors_for_a_valid_command()
    {
        var command = new UpdateUserRoleCommand(Guid.NewGuid(), UserRole.Manager);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion
}
