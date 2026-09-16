using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Roster.UpdateRosterSlot;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Roster.UpdateRosterSlot;

public class UpdateRosterSlotCommandValidatorTests
{
	#region Private Fields

	private readonly UpdateRosterSlotCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_have_error_when_the_user_id_is_empty()
	{
		var result = _validator.TestValidate(new UpdateRosterSlotCommand(Guid.Empty, RosterSlot.Main));

		result.ShouldHaveValidationErrorFor(x => x.UserId);
	}

	[Fact]
	public void Should_not_have_errors_when_clearing_the_slot()
	{
		var result = _validator.TestValidate(new UpdateRosterSlotCommand(Guid.NewGuid(), null));

		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void Should_not_have_errors_for_a_valid_slot()
	{
		var result = _validator.TestValidate(new UpdateRosterSlotCommand(Guid.NewGuid(), RosterSlot.Bench));

		result.ShouldNotHaveAnyValidationErrors();
	}

	#endregion
}
