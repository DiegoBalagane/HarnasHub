using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Veto.GetVetoSuggestion;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Veto.GetVetoSuggestion;

public class GetVetoSuggestionQueryValidatorTests
{
	#region Private Fields

	private readonly GetVetoSuggestionQueryValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_have_error_when_opponent_is_empty()
	{
		_validator.TestValidate(new GetVetoSuggestionQuery(" ")).ShouldHaveValidationErrorFor(x => x.OpponentName);
	}

	[Fact]
	public void Should_not_have_errors_for_a_valid_opponent()
	{
		_validator.TestValidate(new GetVetoSuggestionQuery("Team X")).ShouldNotHaveAnyValidationErrors();
	}

	#endregion
}
