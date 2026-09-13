using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Auth.DiscordLogin;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Auth.DiscordLogin;

public class DiscordLoginCommandValidatorTests
{
    #region Private Fields

    private readonly DiscordLoginCommandValidator _validator = new();

    #endregion

    #region Public Methods

    [Fact]
    public void Should_have_error_when_code_is_empty()
    {
        var command = new DiscordLoginCommand(string.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Should_not_have_errors_for_a_valid_command()
    {
        var command = new DiscordLoginCommand("some-oauth-code");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion
}
