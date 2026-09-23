using HarnasHub.Application.Features.Attendance.GetIncidents;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Attendance.GetIncidents;

public class GetIncidentsHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_return_incidents_newest_first()
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
			OccurredOn = new DateOnly(2026, 9, 10),
			RecordedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow
		});
		dbContext.AttendanceIncidents.Add(new AttendanceIncident
		{
			Id = Guid.NewGuid(),
			UserId = playerId,
			Type = AttendanceIncidentType.Absent,
			OccurredOn = new DateOnly(2026, 9, 20),
			RecordedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetIncidentsHandler(dbContext);
		var result = await handler.Handle(new GetIncidentsQuery(null), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(2, result.Value.Count);
		Assert.Equal(new DateOnly(2026, 9, 20), result.Value[0].OccurredOn);
		Assert.Equal("Zenek", result.Value[0].PlayerName);
	}

	[Fact]
	public async Task Should_filter_by_user_id()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var (playerA, playerB) = (Guid.NewGuid(), Guid.NewGuid());
		dbContext.AttendanceIncidents.Add(new AttendanceIncident
		{
			Id = Guid.NewGuid(),
			UserId = playerA,
			Type = AttendanceIncidentType.Late,
			OccurredOn = new DateOnly(2026, 9, 10),
			RecordedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow
		});
		dbContext.AttendanceIncidents.Add(new AttendanceIncident
		{
			Id = Guid.NewGuid(),
			UserId = playerB,
			Type = AttendanceIncidentType.Absent,
			OccurredOn = new DateOnly(2026, 9, 11),
			RecordedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetIncidentsHandler(dbContext);
		var result = await handler.Handle(new GetIncidentsQuery(playerA), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(playerA, result.Value.Single().UserId);
	}

	[Fact]
	public async Task Should_label_an_incident_for_a_deleted_user()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.AttendanceIncidents.Add(new AttendanceIncident
		{
			Id = Guid.NewGuid(),
			UserId = Guid.NewGuid(),
			Type = AttendanceIncidentType.Late,
			OccurredOn = new DateOnly(2026, 9, 10),
			RecordedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetIncidentsHandler(dbContext);
		var result = await handler.Handle(new GetIncidentsQuery(null), CancellationToken.None);

		Assert.Equal("Usunięty zawodnik", result.Value.Single().PlayerName);
	}

	#endregion
}
