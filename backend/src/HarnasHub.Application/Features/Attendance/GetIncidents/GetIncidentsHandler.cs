using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Attendance.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Attendance.GetIncidents;

/// <summary>Handles <see cref="GetIncidentsQuery"/>.</summary>
public class GetIncidentsHandler(IApplicationDbContext dbContext)
	: IRequestHandler<GetIncidentsQuery, ErrorOr<List<AttendanceIncidentDto>>>
{
	#region Public Methods

	public async Task<ErrorOr<List<AttendanceIncidentDto>>> Handle(GetIncidentsQuery request, CancellationToken cancellationToken)
	{
		var rows = await (
			from incident in dbContext.AttendanceIncidents
			where request.UserId == null || incident.UserId == request.UserId
			// Left join: a departed player's incident history stays even after their account is deleted.
			join user in dbContext.Users on incident.UserId equals user.Id into userGroup
			from user in userGroup.DefaultIfEmpty()
			orderby incident.OccurredOn descending, incident.CreatedAtUtc descending
			select new { incident, user })
			.ToListAsync(cancellationToken);

		return rows
			.Select(row => new AttendanceIncidentDto(
				row.incident.Id,
				row.incident.UserId,
				row.user != null ? (row.user.InGameNickname ?? row.user.DisplayName) : "Usunięty zawodnik",
				row.incident.Type.ToString(),
				row.incident.OccurredOn,
				row.incident.Note,
				row.incident.CreatedAtUtc))
			.ToList();
	}

	#endregion
}
