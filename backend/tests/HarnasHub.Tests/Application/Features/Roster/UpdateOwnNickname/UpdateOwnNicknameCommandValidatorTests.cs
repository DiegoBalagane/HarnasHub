using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Roster.UpdateOwnNickname;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Roster.UpdateOwnNickname;

public class UpdateOwnNicknameCommandValidatorTests
{
	#region Private Fields

	private readonly UpdateOwnNicknameCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_have_error_when_the_nickname_is_too_short()
	{
		var result = _validator.TestValidate(new UpdateOwnNicknameCommand("a"));

		result.ShouldHaveValidationErrorFor(x => x.Nickname);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Should_not_have_errors_for_a_blank_nickname_since_it_means_clearing_it(string? nickname)
	{
		var result = _validator.TestValidate(new UpdateOwnNicknameCommand(nickname));

		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void Should_have_error_when_the_nickname_is_too_long()
	{
		var result = _validator.TestValidate(new UpdateOwnNicknameCommand(new string('x', 33)));

		result.ShouldHaveValidationErrorFor(x => x.Nickname);
	}

	[Fact]
	public void Should_not_have_errors_for_a_valid_nickname()
	{
		var result = _validator.TestValidate(new UpdateOwnNicknameCommand("s1mple"));

		result.ShouldNotHaveAnyValidationErrors();
	}

	#endregion
}
