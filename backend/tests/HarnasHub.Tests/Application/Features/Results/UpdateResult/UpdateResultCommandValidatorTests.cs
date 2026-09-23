using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Results.UpdateResult;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Results.UpdateResult;

public class UpdateResultCommandValidatorTests
{
	#region Private Fields

	private readonly UpdateResultCommandValidator _validator = new();

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
	public void Should_have_error_when_a_score_is_negative()
	{
		var command = Command(ourScore: -1);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.OurScore);
	}

	[Fact]
	public void Should_have_error_when_demo_url_is_malformed()
	{
		var command = Command(demoUrl: "not-a-url");

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.DemoUrl);
	}

	[Fact]
	public void Should_have_error_when_tournament_category_has_no_tournament_id()
	{
		var command = Command(category: MatchCategory.Tournament, tournamentId: null);

		var result = _validator.TestValidate(command);

		result.ShouldHaveValidationErrorFor(x => x.TournamentId);
	}

	[Fact]
	public void Should_not_have_errors_for_a_valid_scrimmage()
	{
		var command = Command();

		var result = _validator.TestValidate(command);

		result.ShouldNotHaveAnyValidationErrors();
	}

	#endregion

	#region Private Methods

	private static UpdateResultCommand Command(
		string opponent = "Team X",
		string? demoUrl = null,
		MatchCategory category = MatchCategory.Scrimmage,
		Guid? tournamentId = null,
		Guid? leagueId = null,
		int ourScore = 16,
		int opponentScore = 10) =>
		new(Guid.NewGuid(), opponent, ourScore, opponentScore, "Mirage", demoUrl, "Dobry mecz", DateTime.UtcNow, category, tournamentId, leagueId);

	#endregion
}
