using HarnasHub.Application.Features.Calendar.GetEventAvailability;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Calendar.GetEventAvailability;

public class GetEventAvailabilityHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_return_not_found_for_a_missing_event()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new GetEventAvailabilityHandler(dbContext);

		var result = await handler.Handle(new GetEventAvailabilityQuery(Guid.NewGuid()), CancellationToken.None);

		Assert.True(result.IsError);
	}

	[Fact]
	public async Task Should_expose_each_members_own_nickname_alongside_their_discord_name()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var eventId = Guid.NewGuid();
		var userId = Guid.NewGuid();
		dbContext.Events.Add(new Event
		{
			Id = eventId,
			Title = "Trening",
			Type = EventType.Training,
			StartsAtUtc = DateTime.UtcNow,
			CreatedByUserId = userId,
			CreatedAtUtc = DateTime.UtcNow
		});
		dbContext.Users.Add(new User
		{
			Id = userId,
			DiscordId = userId.ToString(),
			DisplayName = "DiscordowaNazwa",
			InGameNickname = "Zenus",
			Role = UserRole.Player,
			CreatedAtUtc = DateTime.UtcNow
		});
		dbContext.Availabilities.Add(new HarnasHub.Core.Entities.Availability
		{
			Id = Guid.NewGuid(),
			EventId = eventId,
			UserId = userId,
			Status = AvailabilityStatus.Available,
			UpdatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetEventAvailabilityHandler(dbContext);

		var result = await handler.Handle(new GetEventAvailabilityQuery(eventId), CancellationToken.None);

		var member = Assert.Single(result.Value);
		Assert.Equal("DiscordowaNazwa", member.DisplayName);
		Assert.Equal("Zenus", member.InGameNickname);
		Assert.Equal(nameof(AvailabilityStatus.Available), member.Status);
	}

	#endregion
}
