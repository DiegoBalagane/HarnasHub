using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Nades.AddNade;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Nades.AddNade;

public class AddNadeCommandValidatorTests
{
    #region Private Fields

    private readonly AddNadeCommandValidator _validator = new();

    #endregion

    #region Public Methods

    [Fact]
    public void Should_have_error_when_map_name_is_empty()
    {
        var command = new AddNadeCommand(string.Empty, GrenadeType.Smoke, "Mid smoke", null, null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.MapName);
    }

    [Fact]
    public void Should_have_error_when_youtube_url_is_malformed()
    {
        var command = new AddNadeCommand("Mirage", GrenadeType.Smoke, "Mid smoke", null, "not-a-url");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.YoutubeUrl);
    }

    [Fact]
    public void Should_not_have_errors_for_a_valid_command()
    {
        var command = new AddNadeCommand("Mirage", GrenadeType.Smoke, "Mid smoke", "Z T spawn", "https://youtube.com/watch?v=abc");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion
}
