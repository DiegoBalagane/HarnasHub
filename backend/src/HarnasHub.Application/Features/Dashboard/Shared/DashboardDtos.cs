using HarnasHub.Application.Features.Calendar.Shared;

namespace HarnasHub.Application.Features.Dashboard.Shared;

/// <summary>Summary shown on the team dashboard for the current user.</summary>
public record DashboardSummaryDto(EventDto? NextEvent, int OpenTaskCount);
