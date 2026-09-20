using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Tournaments.CreateTournament;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Tournaments.CreateTournament;

public class CreateTournamentCommandValidatorTests
{
	#region Private Fields

	private readonly CreateTournamentCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_have_error_when_name_is_empty()
	{
		var result = _validator.TestValidate(new CreateTournamentCommand(string.Empty));

		result.ShouldHaveValidationErrorFor(x => x.Name);
	}

	[Fact]
	public void Should_have_error_when_name_is_too_long()
	{
		var result = _validator.TestValidate(new CreateTournamentCommand(new string('a', 101)));

		result.ShouldHaveValidationErrorFor(x => x.Name);
	}

	[Fact]
	public void Should_not_have_errors_for_a_valid_name()
	{
		var result = _validator.TestValidate(new CreateTournamentCommand("Blast Q1"));

		result.ShouldNotHaveAnyValidationErrors();
	}

	#endregion
}
