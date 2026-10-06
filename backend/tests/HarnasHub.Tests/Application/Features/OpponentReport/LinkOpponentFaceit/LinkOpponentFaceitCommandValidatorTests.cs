using FluentValidation.TestHelper;
using HarnasHub.Application.Features.OpponentReport.LinkOpponentFaceit;
using Xunit;

namespace HarnasHub.Tests.Application.Features.OpponentReport.LinkOpponentFaceit;

public class LinkOpponentFaceitCommandValidatorTests
{
	#region Private Fields

	private readonly LinkOpponentFaceitCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_have_error_when_opponent_is_empty()
	{
		_validator.TestValidate(new LinkOpponentFaceitCommand(" ", "alpha")).ShouldHaveValidationErrorFor(x => x.OpponentName);
	}

	[Theory]
	[InlineData("")]
	[InlineData("https://www.faceit.com/en/players/alpha")]
	[InlineData("not a valid nick!")]
	public void Should_have_error_for_an_unrecognised_source(string source)
	{
		_validator.TestValidate(new LinkOpponentFaceitCommand("Team X", source)).ShouldHaveValidationErrorFor(x => x.Source);
	}

	[Theory]
	[InlineData("alpha, beta, gamma")]
	[InlineData("https://www.faceit.com/en/teams/0b1c2d3e-1111-2222-3333-444455556666")]
	[InlineData("https://www.faceit.com/en/cs2/room/1-0b1c2d3e-1111-2222-3333-444455556666")]
	public void Should_not_have_errors_for_valid_sources(string source)
	{
		_validator.TestValidate(new LinkOpponentFaceitCommand("Team X", source)).ShouldNotHaveAnyValidationErrors();
	}

	#endregion
}
