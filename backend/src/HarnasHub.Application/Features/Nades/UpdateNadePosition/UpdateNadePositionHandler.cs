using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Nades.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Nades.UpdateNadePosition;

/// <summary>Handles <see cref="UpdateNadePositionCommand"/>. Only the entry's author or Coach/Manager may move or clear its pin.</summary>
public class UpdateNadePositionHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<UpdateNadePositionCommand, ErrorOr<NadeEntryDto>>
{
	#region Public Methods

	public async Task<ErrorOr<NadeEntryDto>> Handle(UpdateNadePositionCommand request, CancellationToken cancellationToken)
	{
		var entry = await dbContext.NadeEntries.FirstOrDefaultAsync(n => n.Id == request.NadeId, cancellationToken);

		if (entry is null)
		{
			return NadeErrors.NotFound;
		}

		var isOwner = entry.CreatedByUserId == currentUser.UserId;
		var isCoachOrManager = currentUser.Role == "Manager" || currentUser.IsCoach;

		if (!isOwner && !isCoachOrManager)
		{
			return NadeErrors.NotYourEntry;
		}

		entry.LandingX = request.X;
		entry.LandingY = request.Y;

		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("nades", cancellationToken);

		return new NadeEntryDto(
			entry.Id, entry.MapName, entry.Type.ToString(), entry.Title, entry.Description, entry.YoutubeUrl,
			entry.LandingX, entry.LandingY, entry.CreatedByUserId);
	}

	#endregion
}
