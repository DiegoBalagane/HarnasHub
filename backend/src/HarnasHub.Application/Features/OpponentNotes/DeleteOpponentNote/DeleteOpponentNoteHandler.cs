using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentNotes.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.OpponentNotes.DeleteOpponentNote;

/// <summary>Handles <see cref="DeleteOpponentNoteCommand"/>.</summary>
public class DeleteOpponentNoteHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<DeleteOpponentNoteCommand, ErrorOr<Success>>
{
	#region Public Methods

	public async Task<ErrorOr<Success>> Handle(DeleteOpponentNoteCommand request, CancellationToken cancellationToken)
	{
		var note = await dbContext.OpponentNotes.FirstOrDefaultAsync(n => n.Id == request.NoteId, cancellationToken);

		if (note is null)
		{
			return OpponentNoteErrors.NoteNotFound;
		}

		dbContext.OpponentNotes.Remove(note);
		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("opponents", cancellationToken);

		return Result.Success;
	}

	#endregion
}
