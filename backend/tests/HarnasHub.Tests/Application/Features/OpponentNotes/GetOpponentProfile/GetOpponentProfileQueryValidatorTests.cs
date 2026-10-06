using FluentValidation.TestHelper;
using HarnasHub.Application.Features.OpponentNotes.GetOpponentProfile;
using Xunit;

namespace HarnasHub.Tests.Application.Features.OpponentNotes.GetOpponentProfile;

public class GetOpponentProfileQueryValidatorTests
{
	#region Private Fields

	private readonly GetOpponentProfileQueryValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_have_error_when_name_is_empty()
	{
		var result = _validator.TestValidate(new GetOpponentProfileQuery(""));

		result.ShouldHaveValidationErrorFor(x => x.Name);
	}

	[Fact]
	public void Should_have_error_when_name_is_too_long()
	{
		var result = _validator.TestValidate(new GetOpponentProfileQuery(new string('x', 101)));

		result.ShouldHaveValidationErrorFor(x => x.Name);
	}

	[Fact]
	public void Should_not_have_errors_for_a_valid_name()
	{
		var result = _validator.TestValidate(new GetOpponentProfileQuery("Team X"));

		result.ShouldNotHaveAnyValidationErrors();
	}

	#endregion
}
