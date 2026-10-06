using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.GamePlan.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.GamePlan.GetEventGamePlan;

/// <summary>Handles <see cref="GetEventGamePlanQuery"/>.</summary>
public class GetEventGamePlanHandler(IApplicationDbContext dbContext)
	: IRequestHandler<GetEventGamePlanQuery, ErrorOr<EventGamePlanDto>>
{
	#region Public Methods

	public async Task<ErrorOr<EventGamePlanDto>> Handle(GetEventGamePlanQuery request, CancellationToken cancellationToken)
	{
		if (!await dbContext.Events.AnyAsync(e => e.Id == request.EventId, cancellationToken))
		{
			return GamePlanErrors.EventNotFound;
		}

		return await GamePlanReader.ReadAsync(dbContext, request.EventId, cancellationToken);
	}

	#endregion
}
