using ErrorOr;
using HarnasHub.Application.Features.Dashboard.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Dashboard.GetDashboardSummary;

/// <summary>Returns the next upcoming event and the current user's open task count.</summary>
public record GetDashboardSummaryQuery : IRequest<ErrorOr<DashboardSummaryDto>>;
