using FluentValidation.TestHelper;
using HarnasHub.Application.Features.OpponentReport.SyncOpponentFaceit;
using Xunit;

namespace HarnasHub.Tests.Application.Features.OpponentReport.SyncOpponentFaceit;

public class SyncOpponentFaceitCommandValidatorTests
{
	#region Private Fields

	private readonly SyncOpponentFaceitCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_have_error_when_opponent_is_empty()
	{
		_validator.TestValidate(new SyncOpponentFaceitCommand("", true)).ShouldHaveValidationErrorFor(x => x.OpponentName);
	}

	[Fact]
	public void Should_have_error_when_opponent_is_too_long()
	{
		_validator.TestValidate(new SyncOpponentFaceitCommand(new string('x', 101), true)).ShouldHaveValidationErrorFor(x => x.OpponentName);
	}

	[Fact]
	public void Should_not_have_errors_for_a_valid_opponent()
	{
		_validator.TestValidate(new SyncOpponentFaceitCommand("Team X", false)).ShouldNotHaveAnyValidationErrors();
	}

	#endregion
}
