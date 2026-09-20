using HarnasHub.Application.Features.Nades.UpdateNadePosition;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Nades.UpdateNadePosition;

public class UpdateNadePositionCommandValidatorTests
{
	#region Private Fields

	private readonly UpdateNadePositionCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Theory]
	[InlineData(0f, 0f)]
	[InlineData(0.5f, 0.5f)]
	[InlineData(1f, 1f)]
	public void Should_accept_coordinates_inside_the_radar(float x, float y)
	{
		var result = _validator.Validate(new UpdateNadePositionCommand(Guid.NewGuid(), x, y));

		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_accept_both_coordinates_null_as_clearing_the_pin()
	{
		var result = _validator.Validate(new UpdateNadePositionCommand(Guid.NewGuid(), null, null));

		Assert.True(result.IsValid);
	}

	[Theory]
	[InlineData(-0.01f, 0.5f)]
	[InlineData(1.01f, 0.5f)]
	[InlineData(0.5f, -1f)]
	[InlineData(0.5f, 2f)]
	public void Should_reject_coordinates_outside_the_radar(float x, float y)
	{
		var result = _validator.Validate(new UpdateNadePositionCommand(Guid.NewGuid(), x, y));

		Assert.False(result.IsValid);
	}

	[Theory]
	[InlineData(0.5f, null)]
	[InlineData(null, 0.5f)]
	public void Should_reject_only_one_coordinate_set(float? x, float? y)
	{
		var result = _validator.Validate(new UpdateNadePositionCommand(Guid.NewGuid(), x, y));

		Assert.False(result.IsValid);
	}

	#endregion
}
