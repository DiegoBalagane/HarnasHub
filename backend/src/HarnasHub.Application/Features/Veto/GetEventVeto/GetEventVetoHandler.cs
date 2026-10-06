using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Veto.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Veto.GetEventVeto;

/// <summary>Handles <see cref="GetEventVetoQuery"/>.</summary>
public class GetEventVetoHandler(IApplicationDbContext dbContext)
	: IRequestHandler<GetEventVetoQuery, ErrorOr<List<VetoStepDto>>>
{
	#region Public Methods

	public async Task<ErrorOr<List<VetoStepDto>>> Handle(GetEventVetoQuery request, CancellationToken cancellationToken)
	{
		if (!await dbContext.Events.AnyAsync(e => e.Id == request.EventId, cancellationToken))
		{
			return VetoErrors.EventNotFound;
		}

		return await dbContext.EventVetoSteps
			.Where(s => s.EventId == request.EventId)
			.OrderBy(s => s.Order)
			.Select(s => new VetoStepDto(s.Order, s.Actor.ToString(), s.Action.ToString(), s.MapName.ToString()))
			.ToListAsync(cancellationToken);
	}

	#endregion
}
