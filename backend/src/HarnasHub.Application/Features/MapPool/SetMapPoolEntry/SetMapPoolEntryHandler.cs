using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.MapPool.SetMapPoolEntry;

/// <summary>Handles <see cref="SetMapPoolEntryCommand"/> as an upsert keyed by map; a null status removes the entry.</summary>
public class SetMapPoolEntryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<SetMapPoolEntryCommand, ErrorOr<Success>>
{
	#region Public Methods

	public async Task<ErrorOr<Success>> Handle(SetMapPoolEntryCommand request, CancellationToken cancellationToken)
	{
		var entry = await dbContext.MapPoolEntries.FirstOrDefaultAsync(e => e.MapName == request.MapName, cancellationToken);

		if (request.Status is not { } status)
		{
			if (entry is not null)
			{
				dbContext.MapPoolEntries.Remove(entry);
			}
		}
		else
		{
			if (entry is null)
			{
				entry = new MapPoolEntry { Id = Guid.NewGuid(), MapName = request.MapName };
				dbContext.MapPoolEntries.Add(entry);
			}

			entry.Status = status;
			entry.Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
			entry.UpdatedByUserId = currentUser.UserId;
			entry.UpdatedAtUtc = DateTime.UtcNow;
		}

		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("map-pool", cancellationToken);

		return Result.Success;
	}

	#endregion
}
