using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Calendar.UpdateEvent;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Calendar.UpdateEvent;

public class UpdateEventCommandValidatorTests
{
	#region Private Fields

	private readonly UpdateEventCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_have_error_when_event_id_is_empty()
	{
		var command = new UpdateEventCommand(Guid.Empty, "Trening", EventType.Training, DateTime.UtcNow, null, null, null);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.EventId);
	}

	[Fact]
	public void Should_have_error_when_title_is_empty()
	{
		var command = new UpdateEventCommand(Guid.NewGuid(), string.Empty, EventType.Training, DateTime.UtcNow, null, null, null);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.Title);
	}

	[Fact]
	public void Should_have_error_when_url_is_not_a_well_formed_uri()
	{
		var command = new UpdateEventCommand(Guid.NewGuid(), "Trening", EventType.Training, DateTime.UtcNow, null, "nie-url", null);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.Url);
	}

	[Fact]
	public void Should_not_have_errors_for_a_valid_command()
	{
		var command = new UpdateEventCommand(
			Guid.NewGuid(),
			"Trening",
			EventType.Training,
			DateTime.UtcNow,
			"Warszawa",
			"https://pracc.com/matches/3338510",
			null);

		var result = _validator.TestValidate(command);

		result.ShouldNotHaveAnyValidationErrors();
	}

	#endregion
}
