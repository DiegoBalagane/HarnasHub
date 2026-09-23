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
		var command = new CreateEventCommand(string.Empty, EventType.Training, DateTime.UtcNow, null, null, null, null);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.Title);
	}

	[Fact]
	public void Should_have_error_when_type_is_invalid()
	{
		var command = new CreateEventCommand("Trening", (EventType)999, DateTime.UtcNow, null, null, null, null);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.Type);
	}

	[Fact]
	public void Should_have_error_when_url_is_not_a_well_formed_uri()
	{
		var command = new CreateEventCommand("Trening", EventType.Training, DateTime.UtcNow, null, null, "nie-url", null);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.Url);
	}

	[Fact]
	public void Should_not_have_errors_for_a_valid_command()
	{
		var command = new CreateEventCommand(
			"Trening",
			EventType.Training,
			DateTime.UtcNow,
			null,
			"Warszawa",
			"https://pracc.com/matches/3338510",
			null);

		var result = _validator.TestValidate(command);

		result.ShouldNotHaveAnyValidationErrors();
	}

	#endregion
}
