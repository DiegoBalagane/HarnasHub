using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentNotes.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.OpponentNotes.GetOpponentNotes;

/// <summary>Handles <see cref="GetOpponentNotesQuery"/>.</summary>
public class GetOpponentNotesHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetOpponentNotesQuery, ErrorOr<List<OpponentNoteDto>>>
{
    #region Public Methods

    public async Task<ErrorOr<List<OpponentNoteDto>>> Handle(GetOpponentNotesQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.OpponentNotes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.OpponentName))
        {
            query = query.Where(n => n.OpponentName == request.OpponentName);
        }

        return await query
            .OrderByDescending(n => n.CreatedAtUtc)
            .Select(n => new OpponentNoteDto(n.Id, n.OpponentName, n.Content, n.MaterialUrl, n.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    #endregion
}
