using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Calendar.SetAvailability;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Calendar.SetAvailability;

public class SetAvailabilityCommandValidatorTests
{
	#region Private Fields

	private readonly SetAvailabilityCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_have_error_when_event_id_is_empty()
	{
		var command = new SetAvailabilityCommand(Guid.Empty, AvailabilityStatus.Available);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.EventId);
	}

	[Fact]
	public void Should_not_have_errors_for_a_valid_command()
	{
		var command = new SetAvailabilityCommand(Guid.NewGuid(), AvailabilityStatus.Maybe);

		var result = _validator.TestValidate(command);

		result.ShouldNotHaveAnyValidationErrors();
	}

	#endregion
}
