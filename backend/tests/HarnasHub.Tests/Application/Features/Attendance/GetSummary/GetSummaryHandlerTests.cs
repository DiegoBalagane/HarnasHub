using HarnasHub.Application.Features.Attendance.GetSummary;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Attendance.GetSummary;

public class GetSummaryHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_include_a_roster_player_with_zero_incidents()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var playerId = Guid.NewGuid();
		dbContext.Users.Add(new User
		{
			Id = playerId,
			DiscordId = playerId.ToString(),
			DisplayName = "Zenek",
			AccessLevel = AccessLevel.Player,
			RosterSlot = RosterSlot.Main,
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetSummaryHandler(dbContext);
		var result = await handler.Handle(new GetSummaryQuery(), CancellationToken.None);

		var entry = result.Value.Single();
		Assert.Equal("Zenek", entry.PlayerName);
		Assert.Equal(0, entry.LateCount);
		Assert.Equal(0, entry.AbsentCount);
	}

	[Fact]
	public async Task Should_count_late_and_absent_separately_per_player()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var playerId = Guid.NewGuid();
		dbContext.Users.Add(new User
		{
			Id = playerId,
			DiscordId = playerId.ToString(),
			DisplayName = "Zenek",
			AccessLevel = AccessLevel.Player,
			RosterSlot = RosterSlot.Main,
			CreatedAtUtc = DateTime.UtcNow
		});
		dbContext.AttendanceIncidents.Add(new AttendanceIncident
		{
			Id = Guid.NewGuid(),
			UserId = playerId,
			Type = AttendanceIncidentType.Late,
			OccurredOn = new DateOnly(2026, 9, 1),
			RecordedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow
		});
		dbContext.AttendanceIncidents.Add(new AttendanceIncident
		{
			Id = Guid.NewGuid(),
			UserId = playerId,
			Type = AttendanceIncidentType.Late,
			OccurredOn = new DateOnly(2026, 9, 5),
			RecordedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow
		});
		dbContext.AttendanceIncidents.Add(new AttendanceIncident
		{
			Id = Guid.NewGuid(),
			UserId = playerId,
			Type = AttendanceIncidentType.Absent,
			OccurredOn = new DateOnly(2026, 9, 10),
			RecordedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetSummaryHandler(dbContext);
		var result = await handler.Handle(new GetSummaryQuery(), CancellationToken.None);

		var entry = result.Value.Single();
		Assert.Equal(2, entry.LateCount);
		Assert.Equal(1, entry.AbsentCount);
	}

	[Fact]
	public async Task Should_exclude_guests_and_stand_ins()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var guestId = Guid.NewGuid();
		dbContext.Users.Add(new User
		{
			Id = guestId,
			DiscordId = guestId.ToString(),
			DisplayName = "Nowy",
			AccessLevel = AccessLevel.Guest,
			CreatedAtUtc = DateTime.UtcNow
		});
		var standInId = Guid.NewGuid();
		dbContext.Users.Add(new User
		{
			Id = standInId,
			DiscordId = standInId.ToString(),
			DisplayName = "Stand-in",
			AccessLevel = AccessLevel.Player,
			RosterSlot = RosterSlot.StandIn,
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetSummaryHandler(dbContext);
		var result = await handler.Handle(new GetSummaryQuery(), CancellationToken.None);

		Assert.Empty(result.Value);
	}

	#endregion
}
