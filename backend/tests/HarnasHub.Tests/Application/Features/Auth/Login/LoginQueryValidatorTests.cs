using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Auth.Login;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Auth.Login;

public class LoginQueryValidatorTests
{
    #region Private Fields

    private readonly LoginQueryValidator _validator = new();

    #endregion

    #region Public Methods

    [Fact]
    public void Should_have_error_when_email_is_empty()
    {
        var query = new LoginQuery(string.Empty, "SecurePass123");

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_have_error_when_password_is_empty()
    {
        var query = new LoginQuery("player@harnashub.com", string.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_not_have_errors_for_valid_credentials()
    {
        var query = new LoginQuery("player@harnashub.com", "SecurePass123");

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion
}
