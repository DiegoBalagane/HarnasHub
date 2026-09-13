using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentNotes.Shared;
using HarnasHub.Core.Entities;
using MediatR;

namespace HarnasHub.Application.Features.OpponentNotes.AddOpponentNote;

/// <summary>Handles <see cref="AddOpponentNoteCommand"/> by persisting the new scouting note.</summary>
public class AddOpponentNoteHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser)
    : IRequestHandler<AddOpponentNoteCommand, ErrorOr<OpponentNoteDto>>
{
    #region Public Methods

    public async Task<ErrorOr<OpponentNoteDto>> Handle(AddOpponentNoteCommand request, CancellationToken cancellationToken)
    {
        var note = new OpponentNote
        {
            Id = Guid.NewGuid(),
            OpponentName = request.OpponentName,
            Content = request.Content,
            MaterialUrl = request.MaterialUrl,
            CreatedByUserId = currentUser.UserId,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.OpponentNotes.Add(note);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new OpponentNoteDto(note.Id, note.OpponentName, note.Content, note.MaterialUrl, note.CreatedAtUtc);
    }

    #endregion
}
