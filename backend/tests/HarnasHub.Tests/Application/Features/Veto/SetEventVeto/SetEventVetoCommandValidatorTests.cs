using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Veto.SetEventVeto;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Veto.SetEventVeto;

public class SetEventVetoCommandValidatorTests
{
	#region Private Fields

	private readonly SetEventVetoCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_have_error_when_a_map_appears_twice()
	{
		var command = new SetEventVetoCommand(Guid.NewGuid(),
		[
			new VetoStepInput(VetoActor.Us, VetoAction.Ban, MapName.Nuke),
			new VetoStepInput(VetoActor.Opponent, VetoAction.Pick, MapName.Nuke)
		]);

		_validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Steps);
	}

	[Fact]
	public void Should_have_error_when_the_decider_is_not_last()
	{
		var command = new SetEventVetoCommand(Guid.NewGuid(),
		[
			new VetoStepInput(VetoActor.Us, VetoAction.Decider, MapName.Nuke),
			new VetoStepInput(VetoActor.Opponent, VetoAction.Ban, MapName.Mirage)
		]);

		_validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Steps);
	}

	[Fact]
	public void Should_have_error_for_an_unknown_map()
	{
		var command = new SetEventVetoCommand(Guid.NewGuid(), [new VetoStepInput(VetoActor.Us, VetoAction.Ban, (MapName)99)]);

		_validator.TestValidate(command).ShouldHaveValidationErrorFor("Steps[0].MapName");
	}

	[Fact]
	public void Should_have_error_for_an_empty_event_id()
	{
		_validator.TestValidate(new SetEventVetoCommand(Guid.Empty, [])).ShouldHaveValidationErrorFor(x => x.EventId);
	}

	[Fact]
	public void Should_accept_a_full_bo1_veto()
	{
		var maps = Enum.GetValues<MapName>();
		var steps = maps
			.Select((map, index) => new VetoStepInput(
				index % 2 == 0 ? VetoActor.Us : VetoActor.Opponent,
				index == maps.Length - 1 ? VetoAction.Decider : VetoAction.Ban,
				map))
			.ToList();

		_validator.TestValidate(new SetEventVetoCommand(Guid.NewGuid(), steps)).ShouldNotHaveAnyValidationErrors();
	}

	#endregion
}
