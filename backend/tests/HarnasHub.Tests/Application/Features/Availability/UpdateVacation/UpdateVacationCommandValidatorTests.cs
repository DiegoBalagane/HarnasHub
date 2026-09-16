using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Availability.UpdateVacation;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Availability.UpdateVacation;

public class UpdateVacationCommandValidatorTests
{
	#region Private Fields

	private readonly UpdateVacationCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_have_error_when_vacation_id_is_empty()
	{
		var command = new UpdateVacationCommand(Guid.Empty, new DateOnly(2026, 9, 20), new DateOnly(2026, 9, 22), null);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.VacationId);
	}

	[Fact]
	public void Should_have_error_when_end_date_is_before_start_date()
	{
		var command = new UpdateVacationCommand(
			Guid.NewGuid(),
			new DateOnly(2026, 9, 22),
			new DateOnly(2026, 9, 20),
			null);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.EndDate);
	}

	[Fact]
	public void Should_not_have_errors_for_a_valid_range()
	{
		var command = new UpdateVacationCommand(
			Guid.NewGuid(),
			new DateOnly(2026, 9, 20),
			new DateOnly(2026, 9, 22),
			"Wesele");

		var result = _validator.TestValidate(command);

		result.ShouldNotHaveAnyValidationErrors();
	}

	#endregion
}
