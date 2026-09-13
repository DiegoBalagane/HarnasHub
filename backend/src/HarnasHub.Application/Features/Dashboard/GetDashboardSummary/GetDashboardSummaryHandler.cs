using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Calendar.Shared;
using HarnasHub.Application.Features.Dashboard.Shared;
using HarnasHub.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Dashboard.GetDashboardSummary;

/// <summary>Handles <see cref="GetDashboardSummaryQuery"/>.</summary>
public class GetDashboardSummaryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser)
    : IRequestHandler<GetDashboardSummaryQuery, ErrorOr<DashboardSummaryDto>>
{
    #region Public Methods

    public async Task<ErrorOr<DashboardSummaryDto>> Handle(
        GetDashboardSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var nextEvent = await dbContext.Events
            .Where(e => e.StartsAtUtc >= now)
            .OrderBy(e => e.StartsAtUtc)
            .Select(e => new EventDto(e.Id, e.Title, e.Type.ToString(), e.StartsAtUtc, e.Location, e.Notes))
            .FirstOrDefaultAsync(cancellationToken);

        var userId = currentUser.UserId;

        var openTaskCount = await dbContext.Tasks
            .CountAsync(t => t.AssignedToUserId == userId && t.Status == TaskItemStatus.Todo, cancellationToken);

        return new DashboardSummaryDto(nextEvent, openTaskCount);
    }

    #endregion
}
