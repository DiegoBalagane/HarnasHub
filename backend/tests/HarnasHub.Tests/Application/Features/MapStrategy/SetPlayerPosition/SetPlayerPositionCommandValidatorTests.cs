using HarnasHub.Application.Features.MapStrategy.SetPlayerPosition;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.MapStrategy.SetPlayerPosition;

public class SetPlayerPositionCommandValidatorTests
{
	#region Private Fields

	private readonly SetPlayerPositionCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Theory]
	[InlineData(0f, 0f)]
	[InlineData(0.5f, 0.5f)]
	[InlineData(1f, 1f)]
	public void Should_accept_coordinates_inside_the_radar(float x, float y)
	{
		var result = _validator.Validate(Command(x, y));

		Assert.True(result.IsValid);
	}

	[Theory]
	[InlineData(-0.01f, 0.5f)]
	[InlineData(1.01f, 0.5f)]
	[InlineData(0.5f, -1f)]
	[InlineData(0.5f, 2f)]
	public void Should_reject_coordinates_outside_the_radar(float x, float y)
	{
		var result = _validator.Validate(Command(x, y));

		Assert.False(result.IsValid);
	}

	[Fact]
	public void Should_reject_an_empty_user_id()
	{
		var command = new SetPlayerPositionCommand(MapName.Inferno, MapSide.T, Guid.Empty, null, 0.5f, 0.5f, null);

		var result = _validator.Validate(command);

		Assert.False(result.IsValid);
	}

	[Fact]
	public void Should_reject_an_unknown_map()
	{
		var command = new SetPlayerPositionCommand((MapName)99, MapSide.T, Guid.NewGuid(), null, 0.5f, 0.5f, null);

		var result = _validator.Validate(command);

		Assert.False(result.IsValid);
	}

	#endregion

	#region Private Methods

	private static SetPlayerPositionCommand Command(float x, float y) =>
		new(MapName.Inferno, MapSide.T, Guid.NewGuid(), "Banana", x, y, null);

	#endregion
}
