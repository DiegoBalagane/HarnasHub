using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Veto.Shared;
using HarnasHub.Core.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Veto.SetEventVeto;

/// <summary>Handles <see cref="SetEventVetoCommand"/> by replacing every stored step of the event in one save.</summary>
public class SetEventVetoHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<SetEventVetoCommand, ErrorOr<List<VetoStepDto>>>
{
	#region Public Methods

	public async Task<ErrorOr<List<VetoStepDto>>> Handle(SetEventVetoCommand request, CancellationToken cancellationToken)
	{
		if (!await dbContext.Events.AnyAsync(e => e.Id == request.EventId, cancellationToken))
		{
			return VetoErrors.EventNotFound;
		}

		var existing = await dbContext.EventVetoSteps
			.Where(s => s.EventId == request.EventId)
			.ToListAsync(cancellationToken);
		dbContext.EventVetoSteps.RemoveRange(existing);

		var steps = request.Steps
			.Select((step, index) => new EventVetoStep
			{
				Id = Guid.NewGuid(),
				EventId = request.EventId,
				Order = index + 1,
				Actor = step.Actor,
				Action = step.Action,
				MapName = step.MapName
			})
			.ToList();
		dbContext.EventVetoSteps.AddRange(steps);

		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("veto", cancellationToken);

		return steps
			.Select(s => new VetoStepDto(s.Order, s.Actor.ToString(), s.Action.ToString(), s.MapName.ToString()))
			.ToList();
	}

	#endregion
}
