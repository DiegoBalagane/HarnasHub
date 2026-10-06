using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentNotes.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.OpponentManagement.UnhideOpponent;

/// <summary>Handles <see cref="UnhideOpponentCommand"/>; unhiding an opponent that is not hidden is a no-op.</summary>
public class UnhideOpponentHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<UnhideOpponentCommand, ErrorOr<Success>>
{
	#region Public Methods

	public async Task<ErrorOr<Success>> Handle(UnhideOpponentCommand request, CancellationToken cancellationToken)
	{
		var key = OpponentNames.ToKey(request.Name);
		var rows = await dbContext.HiddenOpponents.Where(h => h.OpponentKey == key).ToListAsync(cancellationToken);

		if (rows.Count > 0)
		{
			dbContext.HiddenOpponents.RemoveRange(rows);
			await dbContext.SaveChangesAsync(cancellationToken);
			await realtimeNotifier.NotifyAsync("opponents", cancellationToken);
		}

		return Result.Success;
	}

	#endregion
}
