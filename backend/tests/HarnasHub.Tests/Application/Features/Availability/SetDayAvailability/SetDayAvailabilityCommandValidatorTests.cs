using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Availability.SetDayAvailability;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Availability.SetDayAvailability;

public class SetDayAvailabilityCommandValidatorTests
{
	#region Private Fields

	private readonly SetDayAvailabilityCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_have_error_when_partially_available_without_hours()
	{
		var command = new SetDayAvailabilityCommand(
			new DateOnly(2026, 9, 14),
			DayAvailabilityStatus.PartiallyAvailable,
			null,
			null,
			null);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.AvailableFromLocal);
		result.ShouldHaveValidationErrorFor(x => x.AvailableToLocal);
	}

	[Fact]
	public void Should_have_error_when_partially_available_and_from_is_not_before_to()
	{
		var command = new SetDayAvailabilityCommand(
			new DateOnly(2026, 9, 14),
			DayAvailabilityStatus.PartiallyAvailable,
			new TimeOnly(21, 0),
			new TimeOnly(18, 0),
			null);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.AvailableToLocal);
	}

	[Fact]
	public void Should_not_have_errors_for_a_valid_partial_day()
	{
		var command = new SetDayAvailabilityCommand(
			new DateOnly(2026, 9, 14),
			DayAvailabilityStatus.PartiallyAvailable,
			new TimeOnly(18, 0),
			new TimeOnly(21, 0),
			"Wracam później z pracy.");

		var result = _validator.TestValidate(command);

		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void Should_not_require_hours_when_day_is_off()
	{
		var command = new SetDayAvailabilityCommand(new DateOnly(2026, 9, 14), DayAvailabilityStatus.Off, null, null, null);

		var result = _validator.TestValidate(command);

		result.ShouldNotHaveAnyValidationErrors();
	}

	#endregion
}
