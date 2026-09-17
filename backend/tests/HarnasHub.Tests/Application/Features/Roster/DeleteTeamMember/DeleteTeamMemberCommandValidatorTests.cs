using HarnasHub.Application.Features.Roster.DeleteTeamMember;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Roster.DeleteTeamMember;

public class DeleteTeamMemberCommandValidatorTests
{
	#region Private Fields

	private readonly DeleteTeamMemberCommandValidator _validator = new();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_reject_an_empty_user_id()
	{
		var result = _validator.Validate(new DeleteTeamMemberCommand(Guid.Empty));

		Assert.False(result.IsValid);
	}

	[Fact]
	public void Should_accept_a_valid_user_id()
	{
		var result = _validator.Validate(new DeleteTeamMemberCommand(Guid.NewGuid()));

		Assert.True(result.IsValid);
	}

	#endregion
}
