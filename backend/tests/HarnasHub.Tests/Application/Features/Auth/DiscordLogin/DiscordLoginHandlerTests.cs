using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Auth.DiscordLogin;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Auth.DiscordLogin;

public class DiscordLoginHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_create_a_new_account_as_guest_awaiting_a_manager()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = CreateHandler(dbContext, new DiscordProfile("111", "Nowy", "https://cdn/avatar.png"));

		var result = await handler.Handle(new DiscordLoginCommand("code"), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(nameof(AccessLevel.Guest), result.Value.Role);
		var created = await dbContext.Users.SingleAsync();
		Assert.Equal(AccessLevel.Guest, created.AccessLevel);
		Assert.False(created.IsCoach);
	}

	[Fact]
	public async Task Should_keep_the_existing_access_level_and_coach_tag_of_a_returning_member()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.Users.Add(new User
		{
			Id = Guid.NewGuid(),
			DiscordId = "111",
			DisplayName = "Stara nazwa",
			AccessLevel = AccessLevel.Manager,
			IsCoach = true,
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = CreateHandler(dbContext, new DiscordProfile("111", "Nowa nazwa", null));

		var result = await handler.Handle(new DiscordLoginCommand("code"), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(nameof(AccessLevel.Manager), result.Value.Role);

		var returning = await dbContext.Users.SingleAsync();
		Assert.Equal("Nowa nazwa", returning.DisplayName);
		Assert.True(returning.IsCoach);
	}

	[Fact]
	public async Task Should_return_an_error_when_the_discord_exchange_fails()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = CreateHandler(dbContext, profile: null);

		var result = await handler.Handle(new DiscordLoginCommand("code"), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Empty(dbContext.Users);
	}

	#endregion

	#region Private Methods

	private static DiscordLoginHandler CreateHandler(TestApplicationDbContext dbContext, DiscordProfile? profile) =>
		new(dbContext, new StubDiscordOAuthClient(profile), new StubJwtTokenGenerator());

	#endregion

	#region Nested Types

	/// <summary>Returns a fixed Discord profile (or none) instead of calling Discord.</summary>
	private sealed class StubDiscordOAuthClient(DiscordProfile? profile) : IDiscordOAuthClient
	{
		public Task<DiscordProfile?> ExchangeCodeAsync(string code, CancellationToken cancellationToken = default) =>
			Task.FromResult(profile);
	}

	/// <summary>Produces a deterministic placeholder token so the handler can be exercised without signing keys.</summary>
	private sealed class StubJwtTokenGenerator : IJwtTokenGenerator
	{
		public string GenerateToken(User user) => $"token-{user.Id}";
	}

	#endregion
}
