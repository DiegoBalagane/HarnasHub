using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentNotes.Shared;
using HarnasHub.Core.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.OpponentManagement.HideOpponent;

/// <summary>Handles <see cref="HideOpponentCommand"/>; hiding an already hidden opponent is a no-op.</summary>
public class HideOpponentHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<HideOpponentCommand, ErrorOr<Success>>
{
	#region Public Methods

	public async Task<ErrorOr<Success>> Handle(HideOpponentCommand request, CancellationToken cancellationToken)
	{
		var key = OpponentNames.ToKey(request.Name);

		if (!await dbContext.HiddenOpponents.AnyAsync(h => h.OpponentKey == key, cancellationToken))
		{
			dbContext.HiddenOpponents.Add(new HiddenOpponent
			{
				Id = Guid.NewGuid(),
				OpponentKey = key,
				DisplayName = request.Name.Trim(),
				HiddenAtUtc = DateTime.UtcNow,
				HiddenByUserId = currentUser.UserId
			});
			await dbContext.SaveChangesAsync(cancellationToken);
			await realtimeNotifier.NotifyAsync("opponents", cancellationToken);
		}

		return Result.Success;
	}

	#endregion
}
