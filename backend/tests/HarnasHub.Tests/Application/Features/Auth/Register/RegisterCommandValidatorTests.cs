using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Auth.Register;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Auth.Register;

public class RegisterCommandValidatorTests
{
    #region Private Fields

    private readonly RegisterCommandValidator _validator = new();

    #endregion

    #region Public Methods

    [Fact]
    public void Should_have_error_when_email_is_invalid()
    {
        var command = new RegisterCommand("not-an-email", "Player1", "SecurePass123", UserRole.Player);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_have_error_when_password_is_too_short()
    {
        var command = new RegisterCommand("player@harnashub.com", "Player1", "short", UserRole.Player);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_not_have_errors_for_a_valid_command()
    {
        var command = new RegisterCommand("player@harnashub.com", "Player1", "SecurePass123", UserRole.Player);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion
}
