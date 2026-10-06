using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentNotes.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.OpponentNotes.UpdateOpponentNote;

/// <summary>Handles <see cref="UpdateOpponentNoteCommand"/>.</summary>
public class UpdateOpponentNoteHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<UpdateOpponentNoteCommand, ErrorOr<OpponentNoteDto>>
{
	#region Public Methods

	public async Task<ErrorOr<OpponentNoteDto>> Handle(UpdateOpponentNoteCommand request, CancellationToken cancellationToken)
	{
		var note = await dbContext.OpponentNotes.FirstOrDefaultAsync(n => n.Id == request.NoteId, cancellationToken);

		if (note is null)
		{
			return OpponentNoteErrors.NoteNotFound;
		}

		note.OpponentName = request.OpponentName.Trim();
		note.Content = request.Content;
		note.MaterialUrl = string.IsNullOrWhiteSpace(request.MaterialUrl) ? null : request.MaterialUrl;

		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("opponents", cancellationToken);

		return new OpponentNoteDto(note.Id, note.OpponentName, note.Content, note.MaterialUrl, note.CreatedAtUtc);
	}

	#endregion
}
