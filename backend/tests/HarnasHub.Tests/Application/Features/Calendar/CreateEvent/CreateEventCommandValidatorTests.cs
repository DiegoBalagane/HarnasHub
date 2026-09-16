using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Calendar.CreateEvent;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Calendar.CreateEvent;

public class CreateEventCommandValidatorTests
{
	#region Private Fields

	private readonly CreateEventCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_have_error_when_title_is_empty()
	{
		var command = new CreateEventCommand(string.Empty, EventType.Training, DateTime.UtcNow, null, null);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.Title);
	}

	[Fact]
	public void Should_have_error_when_type_is_invalid()
	{
		var command = new CreateEventCommand("Trening", (EventType)999, DateTime.UtcNow, null, null);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.Type);
	}

	[Fact]
	public void Should_not_have_errors_for_a_valid_command()
	{
		var command = new CreateEventCommand("Trening", EventType.Training, DateTime.UtcNow, "Warszawa", null);

		var result = _validator.TestValidate(command);

		result.ShouldNotHaveAnyValidationErrors();
	}

	#endregion
}
