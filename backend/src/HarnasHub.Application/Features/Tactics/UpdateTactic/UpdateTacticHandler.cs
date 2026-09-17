using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Tactics.Shared;
using HarnasHub.Core.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Tactics.UpdateTactic;

/// <summary>Handles <see cref="UpdateTacticCommand"/> by replacing the tactic's metadata and its whole point layout.</summary>
public class UpdateTacticHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<UpdateTacticCommand, ErrorOr<TacticDetailDto>>
{
	#region Public Methods

	public async Task<ErrorOr<TacticDetailDto>> Handle(UpdateTacticCommand request, CancellationToken cancellationToken)
	{
		var tactic = await dbContext.Tactics
			.Include(t => t.Points)
			.FirstOrDefaultAsync(t => t.Id == request.TacticId, cancellationToken);

		if (tactic is null)
		{
			return TacticErrors.NotFound;
		}

		tactic.Name = request.Name;
		tactic.Economy = request.Economy;
		tactic.Note = request.Note;

		// Wholesale replace: the editor sends the full working layout on every save, so the old rows are
		// removed explicitly and the new ones added through the DbSet — adding via the navigation collection
		// would leave EF unable to tell these explicitly-keyed entities apart from already-persisted ones,
		// and it would mark them Modified instead of Added.
		dbContext.TacticPoints.RemoveRange(tactic.Points);
		var newPoints = request.Points.Select((point, index) => new TacticPoint
		{
			Id = Guid.NewGuid(),
			TacticId = tactic.Id,
			Order = index,
			X = point.X,
			Y = point.Y,
			Description = point.Description,
			NadeEntryId = point.NadeEntryId
		}).ToList();
		dbContext.TacticPoints.AddRange(newPoints);

		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("tactics", cancellationToken);

		return new TacticDetailDto(
			tactic.Id,
			tactic.MapName,
			tactic.Side,
			tactic.Name,
			tactic.Economy.ToString(),
			tactic.Note,
			tactic.CreatedByUserId,
			newPoints
				.Select(p => new TacticPointDto(p.Id, p.Order, p.X, p.Y, p.Description, p.NadeEntryId))
				.ToList());
	}

	#endregion
}
