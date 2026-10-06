using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Roster.SetFaceitNickname;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Roster.SetFaceitNickname;

public class SetFaceitNicknameCommandValidatorTests
{
	#region Private Fields

	private readonly SetFaceitNicknameCommandValidator _validator = new();
	private readonly Guid _userId = Guid.NewGuid();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_accept_a_null_value_as_clearing_it()
	{
		_validator.TestValidate(new SetFaceitNicknameCommand(_userId, null)).ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void Should_accept_a_normal_nickname()
	{
		_validator.TestValidate(new SetFaceitNicknameCommand(_userId, "s1mple")).ShouldNotHaveAnyValidationErrors();
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	public void Should_reject_a_blank_nickname(string nickname)
	{
		_validator.TestValidate(new SetFaceitNicknameCommand(_userId, nickname)).ShouldHaveValidationErrorFor(x => x.Nickname);
	}

	[Fact]
	public void Should_reject_a_nickname_longer_than_64_characters()
	{
		_validator.TestValidate(new SetFaceitNicknameCommand(_userId, new string('a', 65))).ShouldHaveValidationErrorFor(x => x.Nickname);
	}

	[Fact]
	public void Should_reject_an_empty_user_id()
	{
		_validator.TestValidate(new SetFaceitNicknameCommand(Guid.Empty, "nick")).ShouldHaveValidationErrorFor(x => x.UserId);
	}

	#endregion
}
