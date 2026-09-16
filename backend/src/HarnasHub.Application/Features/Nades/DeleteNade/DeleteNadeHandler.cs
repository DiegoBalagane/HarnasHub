using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Nades.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Nades.DeleteNade;

/// <summary>Handles <see cref="DeleteNadeCommand"/>.</summary>
public class DeleteNadeHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<DeleteNadeCommand, ErrorOr<Success>>
{
	#region Public Methods

	public async Task<ErrorOr<Success>> Handle(DeleteNadeCommand request, CancellationToken cancellationToken)
	{
		var entry = await dbContext.NadeEntries.FirstOrDefaultAsync(n => n.Id == request.NadeId, cancellationToken);

		if (entry is null)
		{
			return NadeErrors.NotFound;
		}

		var isOwner = entry.CreatedByUserId == currentUser.UserId;
		var isCoachOrManager = currentUser.Role is "Coach" or "Manager";

		if (!isOwner && !isCoachOrManager)
		{
			return NadeErrors.NotYourEntry;
		}

		dbContext.NadeEntries.Remove(entry);
		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("nades", cancellationToken);

		return Result.Success;
	}

	#endregion
}
