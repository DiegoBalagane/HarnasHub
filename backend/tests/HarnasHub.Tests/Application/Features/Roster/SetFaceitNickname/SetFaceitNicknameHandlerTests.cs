using HarnasHub.Application.Features.Roster.SetFaceitNickname;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Roster.SetFaceitNickname;

public class SetFaceitNicknameHandlerTests
{
	#region Private Fields

	private readonly Guid _userId = Guid.NewGuid();

	#endregion

	#region Public Methods

	[Fact]
	public async Task Should_store_the_trimmed_nickname_and_notify_the_roster()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.Users.Add(CreateUser(_userId));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var notifier = new TestRealtimeNotifier();
		var result = await new SetFaceitNicknameHandler(dbContext, notifier)
			.Handle(new SetFaceitNicknameCommand(_userId, "  s1mple  "), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("s1mple", result.Value.FaceitNickname);
		Assert.Equal("s1mple", (await dbContext.Users.SingleAsync()).FaceitNickname);
		Assert.Equal(["roster"], notifier.Topics);
	}

	[Fact]
	public async Task Should_clear_the_nickname_with_a_null_value()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var user = CreateUser(_userId);
		user.FaceitNickname = "s1mple";
		dbContext.Users.Add(user);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await new SetFaceitNicknameHandler(dbContext, new TestRealtimeNotifier())
			.Handle(new SetFaceitNicknameCommand(_userId, null), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Null((await dbContext.Users.SingleAsync()).FaceitNickname);
	}

	[Fact]
	public async Task Should_return_not_found_when_the_user_does_not_exist()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		var result = await new SetFaceitNicknameHandler(dbContext, new TestRealtimeNotifier())
			.Handle(new SetFaceitNicknameCommand(Guid.NewGuid(), "nick"), CancellationToken.None);

		Assert.Equal("Roster.UserNotFound", result.FirstError.Code);
	}

	#endregion

	#region Private Methods

	private static User CreateUser(Guid id) => new()
	{
		Id = id,
		DiscordId = id.ToString("N"),
		DisplayName = "Zawodnik",
		AccessLevel = AccessLevel.Player,
		CreatedAtUtc = DateTime.UtcNow
	};

	#endregion
}
