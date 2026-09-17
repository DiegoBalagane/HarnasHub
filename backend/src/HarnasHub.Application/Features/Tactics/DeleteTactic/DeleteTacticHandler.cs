using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Tactics.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Tactics.DeleteTactic;

/// <summary>Handles <see cref="DeleteTacticCommand"/>.</summary>
public class DeleteTacticHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<DeleteTacticCommand, ErrorOr<Success>>
{
	#region Public Methods

	public async Task<ErrorOr<Success>> Handle(DeleteTacticCommand request, CancellationToken cancellationToken)
	{
		var tactic = await dbContext.Tactics.FirstOrDefaultAsync(t => t.Id == request.TacticId, cancellationToken);

		if (tactic is null)
		{
			return TacticErrors.NotFound;
		}

		dbContext.Tactics.Remove(tactic);
		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("tactics", cancellationToken);

		return Result.Success;
	}

	#endregion
}
