using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Results.AddResult;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Results.AddResult;

public class AddResultCommandValidatorTests
{
	#region Private Fields

	private readonly AddResultCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_have_error_when_opponent_is_empty()
	{
		var command = Command(opponent: string.Empty);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.Opponent);
	}

	[Fact]
	public void Should_have_error_when_demo_url_is_malformed()
	{
		var command = Command(demoUrl: "not-a-url");

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.DemoUrl);
	}

	[Fact]
	public void Should_not_have_errors_for_a_valid_scrimmage()
	{
		var command = Command();

		var result = _validator.TestValidate(command);

		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void Should_have_error_when_tournament_category_has_no_tournament_id()
	{
		var command = Command(category: MatchCategory.Tournament, tournamentId: null);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.TournamentId);
	}

	[Fact]
	public void Should_have_error_when_a_tournament_id_is_set_for_a_scrimmage()
	{
		var command = Command(category: MatchCategory.Scrimmage, tournamentId: Guid.NewGuid());

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.TournamentId);
	}

	[Fact]
	public void Should_have_error_when_league_category_has_no_league_id()
	{
		var command = Command(category: MatchCategory.League, leagueId: null);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.LeagueId);
	}

	[Fact]
	public void Should_have_error_when_a_league_id_is_set_for_a_scrimmage()
	{
		var command = Command(category: MatchCategory.Scrimmage, leagueId: Guid.NewGuid());

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.LeagueId);
	}

	[Fact]
	public void Should_not_have_errors_for_a_valid_tournament_result()
	{
		var command = Command(category: MatchCategory.Tournament, tournamentId: Guid.NewGuid());

		var result = _validator.TestValidate(command);

		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void Should_not_have_errors_for_a_valid_league_result()
	{
		var command = Command(category: MatchCategory.League, leagueId: Guid.NewGuid());

		var result = _validator.TestValidate(command);

		result.ShouldNotHaveAnyValidationErrors();
	}

	#endregion

	#region Private Methods

	private static AddResultCommand Command(
		string opponent = "Team X",
		string? demoUrl = "https://drive.example.com/demo.dem",
		MatchCategory category = MatchCategory.Scrimmage,
		Guid? tournamentId = null,
		Guid? leagueId = null) =>
		new(opponent, 16, 10, "Mirage", demoUrl, "Dobry mecz", DateTime.UtcNow, category, tournamentId, leagueId);

	#endregion
}
