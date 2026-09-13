using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Nades.Shared;
using HarnasHub.Core.Entities;
using MediatR;

namespace HarnasHub.Application.Features.Nades.AddNade;

/// <summary>Handles <see cref="AddNadeCommand"/> by persisting the new nade entry.</summary>
public class AddNadeHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IRealtimeNotifier realtimeNotifier)
    : IRequestHandler<AddNadeCommand, ErrorOr<NadeEntryDto>>
{
    #region Public Methods

    public async Task<ErrorOr<NadeEntryDto>> Handle(AddNadeCommand request, CancellationToken cancellationToken)
    {
        var entry = new NadeEntry
        {
            Id = Guid.NewGuid(),
            MapName = request.MapName,
            Type = request.Type,
            Title = request.Title,
            Description = request.Description,
            YoutubeUrl = request.YoutubeUrl,
            CreatedByUserId = currentUser.UserId,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.NadeEntries.Add(entry);
        await dbContext.SaveChangesAsync(cancellationToken);
        await realtimeNotifier.NotifyAsync("nades", cancellationToken);

        return new NadeEntryDto(
            entry.Id, entry.MapName, entry.Type.ToString(), entry.Title, entry.Description, entry.YoutubeUrl, entry.CreatedByUserId);
    }

    #endregion
}
