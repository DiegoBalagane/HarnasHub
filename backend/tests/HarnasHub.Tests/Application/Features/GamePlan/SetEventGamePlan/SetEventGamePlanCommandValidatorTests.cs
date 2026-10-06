using FluentValidation.TestHelper;
using HarnasHub.Application.Features.GamePlan.SetEventGamePlan;
using Xunit;

namespace HarnasHub.Tests.Application.Features.GamePlan.SetEventGamePlan;

public class SetEventGamePlanCommandValidatorTests
{
	#region Private Fields

	private readonly SetEventGamePlanCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_have_error_for_duplicate_tactics()
	{
		var id = Guid.NewGuid();

		_validator.TestValidate(new SetEventGamePlanCommand(Guid.NewGuid(), null, [id, id], []))
			.ShouldHaveValidationErrorFor(x => x.TacticIds);
	}

	[Fact]
	public void Should_have_error_for_too_many_boards()
	{
		var ids = Enumerable.Range(0, SetEventGamePlanCommandValidator.MaxItemsPerKind + 1).Select(_ => Guid.NewGuid()).ToList();

		_validator.TestValidate(new SetEventGamePlanCommand(Guid.NewGuid(), null, [], ids))
			.ShouldHaveValidationErrorFor(x => x.BoardIds);
	}

	[Fact]
	public void Should_have_error_for_too_long_notes()
	{
		_validator.TestValidate(new SetEventGamePlanCommand(Guid.NewGuid(), new string('x', 4001), [], []))
			.ShouldHaveValidationErrorFor(x => x.Notes);
	}

	[Fact]
	public void Should_have_error_for_an_empty_event_id()
	{
		_validator.TestValidate(new SetEventGamePlanCommand(Guid.Empty, null, [], []))
			.ShouldHaveValidationErrorFor(x => x.EventId);
	}

	[Fact]
	public void Should_accept_a_valid_plan()
	{
		_validator.TestValidate(new SetEventGamePlanCommand(Guid.NewGuid(), "Plan", [Guid.NewGuid()], [Guid.NewGuid()]))
			.ShouldNotHaveAnyValidationErrors();
	}

	#endregion
}
