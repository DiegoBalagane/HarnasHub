using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Attendance.Shared;
using HarnasHub.Core.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Attendance.AddIncident;

/// <summary>Handles <see cref="AddIncidentCommand"/>.</summary>
public class AddIncidentHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<AddIncidentCommand, ErrorOr<AttendanceIncidentDto>>
{
	#region Public Methods

	public async Task<ErrorOr<AttendanceIncidentDto>> Handle(AddIncidentCommand request, CancellationToken cancellationToken)
	{
		var player = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

		if (player is null)
		{
			return AttendanceErrors.PlayerNotFound;
		}

		var incident = new AttendanceIncident
		{
			Id = Guid.NewGuid(),
			UserId = request.UserId,
			Type = request.Type,
			OccurredOn = request.OccurredOn,
			Note = request.Note,
			RecordedByUserId = currentUser.UserId,
			CreatedAtUtc = DateTime.UtcNow
		};

		dbContext.AttendanceIncidents.Add(incident);
		await dbContext.SaveChangesAsync(cancellationToken);

		await realtimeNotifier.NotifyAsync("attendance", cancellationToken);
		await realtimeNotifier.NotifyAsync("dashboard", cancellationToken);

		return new AttendanceIncidentDto(
			incident.Id,
			incident.UserId,
			player.InGameNickname ?? player.DisplayName,
			incident.Type.ToString(),
			incident.OccurredOn,
			incident.Note,
			incident.CreatedAtUtc);
	}

	#endregion
}
