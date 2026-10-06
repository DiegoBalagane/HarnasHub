#region Usings

using HarnasHub.Application.Features.Tactics.ImportTacticFromDemo;
using HarnasHub.Application.Features.Tactics.Shared;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Tests.Application.Features.Tactics.ImportTacticFromDemo;

/// <summary>Covers the validation rules of <see cref="ImportTacticFromDemoCommand"/>.</summary>
public class ImportTacticFromDemoCommandValidatorTests
{
	#region Private Fields

	private readonly ImportTacticFromDemoCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_accept_a_valid_command()
	{
		Assert.True(_validator.Validate(Command([Nade()])).IsValid);
	}

	[Fact]
	public void Should_reject_an_empty_grenade_list()
	{
		var result = _validator.Validate(Command([]));

		Assert.Contains(result.Errors, e => e.ErrorMessage == "Wybierz co najmniej jeden granat.");
	}

	[Fact]
	public void Should_reject_an_empty_name()
	{
		Assert.False(_validator.Validate(Command([Nade()]) with { Name = "" }).IsValid);
	}

	[Theory]
	[InlineData(-0.1f, 0.5f)]
	[InlineData(0.5f, 1.1f)]
	public void Should_reject_landings_outside_the_radar(float landX, float landY)
	{
		Assert.False(_validator.Validate(Command([Nade() with { LandX = landX, LandY = landY }])).IsValid);
	}

	[Fact]
	public void Should_reject_an_unknown_grenade_type_and_missing_thrower()
	{
		var result = _validator.Validate(Command([Nade() with { Type = (GrenadeType)99, ThrowerName = "" }]));

		Assert.Equal(2, result.Errors.Count);
	}

	[Fact]
	public void Should_reject_too_many_grenades()
	{
		var grenades = Enumerable.Repeat(Nade(), ImportTacticFromDemoCommandValidator.MaxGrenades + 1).ToList();

		Assert.False(_validator.Validate(Command(grenades)).IsValid);
	}

	#endregion

	#region Private Methods

	private static ImportTacticFromDemoCommand Command(IReadOnlyList<ImportedNadeInput> grenades) =>
		new(MapName.Mirage, MapSide.CT, "Retake B", EconomyType.FullBuy, null, grenades, true);

	private static ImportedNadeInput Nade() => new(GrenadeType.Smoke, "kacper", 0.1f, 0.2f, 0.5f, 0.6f, 12f);

	#endregion
}
