using HarnasHub.Application.Features.Tactics.CreateTactic;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Tactics.CreateTactic;

public class CreateTacticCommandValidatorTests
{
	#region Private Fields

	private readonly CreateTacticCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_accept_a_valid_command()
	{
		var command = new CreateTacticCommand(MapName.Mirage, MapSide.T, "Eco rush B", EconomyType.Eco, "Wszyscy na B.");

		var result = _validator.Validate(command);

		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_reject_an_empty_name()
	{
		var command = new CreateTacticCommand(MapName.Mirage, MapSide.T, "", EconomyType.Eco, null);

		var result = _validator.Validate(command);

		Assert.False(result.IsValid);
	}

	[Fact]
	public void Should_reject_an_unknown_map()
	{
		var command = new CreateTacticCommand((MapName)99, MapSide.T, "Rush B", EconomyType.Eco, null);

		var result = _validator.Validate(command);

		Assert.False(result.IsValid);
	}

	[Fact]
	public void Should_reject_a_note_over_the_length_limit()
	{
		var command = new CreateTacticCommand(MapName.Mirage, MapSide.T, "Rush B", EconomyType.Eco, new string('a', 501));

		var result = _validator.Validate(command);

		Assert.False(result.IsValid);
	}

	#endregion
}
