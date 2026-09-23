using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Attendance.Shared;
using HarnasHub.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Attendance.GetSummary;

/// <summary>Handles <see cref="GetSummaryQuery"/> by starting from every roster player — the same membership rule as
/// the dashboard's availability lists — so a player with zero incidents still shows up with 0/0, not missing entirely.</summary>
public class GetSummaryHandler(IApplicationDbContext dbContext)
	: IRequestHandler<GetSummaryQuery, ErrorOr<List<AttendanceSummaryEntryDto>>>
{
	#region Public Methods

	public async Task<ErrorOr<List<AttendanceSummaryEntryDto>>> Handle(GetSummaryQuery request, CancellationToken cancellationToken)
	{
		var members = await dbContext.Users
			.Where(user => user.AccessLevel != AccessLevel.Guest && user.RosterSlot != RosterSlot.StandIn)
			.Where(user => user.RosterSlot != null || user.IsCoach)
			.OrderBy(user => user.DisplayName)
			.Select(user => new { user.Id, Name = user.InGameNickname ?? user.DisplayName })
			.ToListAsync(cancellationToken);

		var counts = await dbContext.AttendanceIncidents
			.GroupBy(incident => new { incident.UserId, incident.Type })
			.Select(g => new { g.Key.UserId, g.Key.Type, Count = g.Count() })
			.ToListAsync(cancellationToken);

		var lateByUser = counts.Where(c => c.Type == AttendanceIncidentType.Late).ToDictionary(c => c.UserId, c => c.Count);
		var absentByUser = counts.Where(c => c.Type == AttendanceIncidentType.Absent).ToDictionary(c => c.UserId, c => c.Count);

		return members
			.Select(member => new AttendanceSummaryEntryDto(
				member.Id,
				member.Name,
				lateByUser.GetValueOrDefault(member.Id),
				absentByUser.GetValueOrDefault(member.Id)))
			.OrderByDescending(entry => entry.LateCount + entry.AbsentCount)
			.ToList();
	}

	#endregion
}
