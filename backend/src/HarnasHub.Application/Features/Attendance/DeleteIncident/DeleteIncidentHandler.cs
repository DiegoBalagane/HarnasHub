using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Attendance.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Attendance.DeleteIncident;

/// <summary>Handles <see cref="DeleteIncidentCommand"/>.</summary>
public class DeleteIncidentHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<DeleteIncidentCommand, ErrorOr<Success>>
{
	#region Public Methods

	public async Task<ErrorOr<Success>> Handle(DeleteIncidentCommand request, CancellationToken cancellationToken)
	{
		var incident = await dbContext.AttendanceIncidents
			.FirstOrDefaultAsync(i => i.Id == request.IncidentId, cancellationToken);

		if (incident is null)
		{
			return AttendanceErrors.IncidentNotFound;
		}

		dbContext.AttendanceIncidents.Remove(incident);
		await dbContext.SaveChangesAsync(cancellationToken);

		await realtimeNotifier.NotifyAsync("attendance", cancellationToken);
		await realtimeNotifier.NotifyAsync("dashboard", cancellationToken);

		return Result.Success;
	}

	#endregion
}
