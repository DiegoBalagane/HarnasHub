using HarnasHub.Application.Features.Attendance.AddIncident;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Attendance.AddIncident;

public class AddIncidentHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_persist_an_incident_for_an_existing_player()
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

		var notifier = new TestRealtimeNotifier();
		var recordedBy = Guid.NewGuid();
		var handler = new AddIncidentHandler(dbContext, new TestCurrentUserService(recordedBy), notifier);

		var command = new AddIncidentCommand(playerId, AttendanceIncidentType.Late, new DateOnly(2026, 9, 20), "15 minut");
		var result = await handler.Handle(command, CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("Zenek", result.Value.PlayerName);
		Assert.Equal(nameof(AttendanceIncidentType.Late), result.Value.Type);

		var stored = dbContext.AttendanceIncidents.Single();
		Assert.Equal(playerId, stored.UserId);
		Assert.Equal(recordedBy, stored.RecordedByUserId);
		Assert.Equal(["attendance", "dashboard"], notifier.Topics);
	}

	[Fact]
	public async Task Should_return_player_not_found_for_an_unknown_user()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new AddIncidentHandler(dbContext, new TestCurrentUserService(Guid.NewGuid()), new TestRealtimeNotifier());

		var command = new AddIncidentCommand(Guid.NewGuid(), AttendanceIncidentType.Absent, new DateOnly(2026, 9, 20), null);
		var result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Attendance.PlayerNotFound", result.FirstError.Code);
	}

	#endregion
}
