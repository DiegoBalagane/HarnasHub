using HarnasHub.Application.Features.Attendance.DeleteIncident;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Attendance.DeleteIncident;

public class DeleteIncidentHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_remove_an_existing_incident()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var incidentId = Guid.NewGuid();
		dbContext.AttendanceIncidents.Add(new AttendanceIncident
		{
			Id = incidentId,
			UserId = Guid.NewGuid(),
			Type = AttendanceIncidentType.Late,
			OccurredOn = new DateOnly(2026, 9, 20),
			RecordedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var notifier = new TestRealtimeNotifier();
		var handler = new DeleteIncidentHandler(dbContext, notifier);

		var result = await handler.Handle(new DeleteIncidentCommand(incidentId), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Empty(dbContext.AttendanceIncidents);
		Assert.Equal(["attendance", "dashboard"], notifier.Topics);
	}

	[Fact]
	public async Task Should_return_not_found_for_a_missing_incident()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new DeleteIncidentHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(new DeleteIncidentCommand(Guid.NewGuid()), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Attendance.IncidentNotFound", result.FirstError.Code);
	}

	#endregion
}
