#region Usings

using HarnasHub.Application.Features.OpponentNotes.Shared;
using HarnasHub.Core.Entities;
using HarnasHub.Tests.Common;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.OpponentNotes.Shared;

public class OpponentRevivalTests
{
	#region Public Methods

	[Fact]
	public async Task Should_bring_a_deleted_opponent_back_when_its_name_is_used_again()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.HiddenOpponents.Add(Hidden("team test"));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		await OpponentRevival.ReviveAsync(dbContext, "  Team Test ", CancellationToken.None);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		Assert.Empty(dbContext.HiddenOpponents);
	}

	[Fact]
	public async Task Should_not_bring_it_back_when_an_old_match_is_edited_without_renaming()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.HiddenOpponents.Add(Hidden("team test"));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		await OpponentRevival.ReviveIfRenamedAsync(dbContext, "Team Test", "TEAM TEST", CancellationToken.None);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		Assert.Single(dbContext.HiddenOpponents);
	}

	[Fact]
	public async Task Should_bring_back_the_new_name_when_a_match_is_renamed_to_a_deleted_opponent()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.HiddenOpponents.Add(Hidden("team test"));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		await OpponentRevival.ReviveIfRenamedAsync(dbContext, "Other Team", "Team Test", CancellationToken.None);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		Assert.Empty(dbContext.HiddenOpponents);
	}

	#endregion

	#region Private Methods

	private static HiddenOpponent Hidden(string key) =>
		new() { Id = Guid.NewGuid(), OpponentKey = key, DisplayName = key, HiddenAtUtc = DateTime.UtcNow, HiddenByUserId = Guid.NewGuid() };

	#endregion
}
