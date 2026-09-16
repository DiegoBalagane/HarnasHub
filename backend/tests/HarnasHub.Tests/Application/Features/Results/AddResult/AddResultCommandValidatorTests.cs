using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Results.AddResult;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Results.AddResult;

public class AddResultCommandValidatorTests
{
	#region Private Fields

	private readonly AddResultCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_have_error_when_opponent_is_empty()
	{
		var command = new AddResultCommand(string.Empty, 16, 10, "Mirage", null, null, DateTime.UtcNow);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.Opponent);
	}

	[Fact]
	public void Should_have_error_when_demo_url_is_malformed()
	{
		var command = new AddResultCommand("Team X", 16, 10, "Mirage", "not-a-url", null, DateTime.UtcNow);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.DemoUrl);
	}

	[Fact]
	public void Should_not_have_errors_for_a_valid_command()
	{
		var command = new AddResultCommand("Team X", 16, 10, "Mirage", "https://drive.example.com/demo.dem", "Dobry mecz", DateTime.UtcNow);

		var result = _validator.TestValidate(command);

		result.ShouldNotHaveAnyValidationErrors();
	}

	#endregion
}
