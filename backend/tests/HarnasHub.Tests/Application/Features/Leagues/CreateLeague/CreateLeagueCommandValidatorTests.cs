using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Leagues.CreateLeague;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Leagues.CreateLeague;

public class CreateLeagueCommandValidatorTests
{
	#region Private Fields

	private readonly CreateLeagueCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_have_error_when_name_is_empty()
	{
		var result = _validator.TestValidate(new CreateLeagueCommand(string.Empty, "2026 Wiosna", LeagueType.Online));

		result.ShouldHaveValidationErrorFor(x => x.Name);
	}

	[Fact]
	public void Should_have_error_when_season_is_empty()
	{
		var result = _validator.TestValidate(new CreateLeagueCommand("ESEA", string.Empty, LeagueType.Online));

		result.ShouldHaveValidationErrorFor(x => x.Season);
	}

	[Fact]
	public void Should_have_error_for_an_unknown_type()
	{
		var result = _validator.TestValidate(new CreateLeagueCommand("ESEA", "2026 Wiosna", (LeagueType)99));

		result.ShouldHaveValidationErrorFor(x => x.Type);
	}

	[Fact]
	public void Should_not_have_errors_for_a_valid_command()
	{
		var result = _validator.TestValidate(new CreateLeagueCommand("ESEA", "2026 Wiosna", LeagueType.Division1));

		result.ShouldNotHaveAnyValidationErrors();
	}

	#endregion
}
