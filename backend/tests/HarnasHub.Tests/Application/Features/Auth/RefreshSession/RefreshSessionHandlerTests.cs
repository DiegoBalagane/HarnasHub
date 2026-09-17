using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Auth.RefreshSession;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Auth.RefreshSession;

public class RefreshSessionHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_issue_a_new_token_reflecting_the_users_current_role()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var user = new User
		{
			Id = Guid.NewGuid(),
			DiscordId = "111",
			DisplayName = "Nowy Zawodnik",
			AccessLevel = AccessLevel.Player,
			CreatedAtUtc = DateTime.UtcNow
		};
		dbContext.Users.Add(user);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new RefreshSessionHandler(dbContext, new TestCurrentUserService(user.Id), new StubJwtTokenGenerator());

		var result = await handler.Handle(new RefreshSessionCommand(), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(nameof(AccessLevel.Player), result.Value.Role);
		Assert.Equal($"token-{user.Id}-{AccessLevel.Player}", result.Value.AccessToken);
	}

	[Fact]
	public async Task Should_return_not_found_when_the_account_no_longer_exists()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new RefreshSessionHandler(dbContext, new TestCurrentUserService(Guid.NewGuid()), new StubJwtTokenGenerator());

		var result = await handler.Handle(new RefreshSessionCommand(), CancellationToken.None);

		Assert.True(result.IsError);
	}

	#endregion

	#region Nested Types

	/// <summary>Produces a token that encodes the role it was generated for, so tests can assert a promotion is picked up.</summary>
	private sealed class StubJwtTokenGenerator : IJwtTokenGenerator
	{
		public string GenerateToken(User user) => $"token-{user.Id}-{user.AccessLevel}";
	}

	#endregion
}
