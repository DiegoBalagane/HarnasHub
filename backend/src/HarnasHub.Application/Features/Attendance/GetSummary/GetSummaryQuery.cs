using ErrorOr;
using HarnasHub.Application.Features.Attendance.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Attendance.GetSummary;

/// <summary>Every roster player's total lateness/absence counts, for the team-wide overview.</summary>
public record GetSummaryQuery : IRequest<ErrorOr<List<AttendanceSummaryEntryDto>>>;
