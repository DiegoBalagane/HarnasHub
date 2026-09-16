using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Availability.SetVacation;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Availability.SetVacation;

public class SetVacationCommandValidatorTests
{
	#region Private Fields

	private readonly SetVacationCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_have_error_when_end_date_is_before_start_date()
	{
		var command = new SetVacationCommand(new DateOnly(2026, 9, 20), new DateOnly(2026, 9, 18), null);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.EndDate);
	}

	[Fact]
	public void Should_not_have_errors_for_a_single_day_vacation()
	{
		var command = new SetVacationCommand(new DateOnly(2026, 9, 20), new DateOnly(2026, 9, 20), "Wesele");

		var result = _validator.TestValidate(command);

		result.ShouldNotHaveAnyValidationErrors();
	}

	#endregion
}
