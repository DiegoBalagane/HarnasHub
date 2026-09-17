using HarnasHub.Application.Features.Tactics.Shared;
using HarnasHub.Application.Features.Tactics.UpdateTactic;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Tactics.UpdateTactic;

public class UpdateTacticCommandValidatorTests
{
	#region Private Fields

	private readonly UpdateTacticCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_accept_a_valid_command_with_points_inside_the_radar()
	{
		var command = Command([new TacticPointInput(0f, 1f, "Smoke z Palace", null)]);

		var result = _validator.Validate(command);

		Assert.True(result.IsValid);
	}

	[Theory]
	[InlineData(-0.01f, 0.5f)]
	[InlineData(1.01f, 0.5f)]
	[InlineData(0.5f, -0.01f)]
	[InlineData(0.5f, 1.01f)]
	public void Should_reject_a_point_outside_the_radar(float x, float y)
	{
		var command = Command([new TacticPointInput(x, y, null, null)]);

		var result = _validator.Validate(command);

		Assert.False(result.IsValid);
	}

	[Fact]
	public void Should_reject_an_empty_name()
	{
		var command = Command([]) with { Name = "" };

		var result = _validator.Validate(command);

		Assert.False(result.IsValid);
	}

	#endregion

	#region Private Methods

	private static UpdateTacticCommand Command(List<TacticPointInput> points) =>
		new(Guid.NewGuid(), "Eco rush B", EconomyType.Eco, "Notatka", points);

	#endregion
}
