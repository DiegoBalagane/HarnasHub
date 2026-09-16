using ErrorOr;
using HarnasHub.Application.Features.Dashboard.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Dashboard.GetDashboardSummary;

/// <summary>Returns the next upcoming event, the current user's open task count, and the team's availability for today and tomorrow.</summary>
public record GetDashboardSummaryQuery : IRequest<ErrorOr<DashboardSummaryDto>>;
