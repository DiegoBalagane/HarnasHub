#region Usings

using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Tactics.Shared;
using HarnasHub.Core.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

#endregion

namespace HarnasHub.Application.Features.Tactics.ImportTacticFromDemo;

/// <summary>Handles <see cref="ImportTacticFromDemoCommand"/>: creates the tactic with one point per grenade landing and,
/// when asked, links every point to a nade library entry — an existing one with a landing within
/// <see cref="DemoNadeFormatter.DuplicateLandingDistance"/>, or a newly created one with landing and throw pins.</summary>
public class ImportTacticFromDemoHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<ImportTacticFromDemoCommand, ErrorOr<TacticDetailDto>>
{
	#region Public Methods

	/// <inheritdoc />
	public async Task<ErrorOr<TacticDetailDto>> Handle(ImportTacticFromDemoCommand request, CancellationToken cancellationToken)
	{
		var now = DateTime.UtcNow;
		var tactic = new Tactic
		{
			Id = Guid.NewGuid(),
			MapName = request.MapName,
			Side = request.Side,
			Name = request.Name,
			Economy = request.Economy,
			Note = request.Note,
			CreatedByUserId = currentUser.UserId,
			CreatedAtUtc = now
		};

		List<NadeEntry> libraryCandidates = request.AddToNadeLibrary
			? await dbContext.NadeEntries
				.Where(n => n.MapName == request.MapName && n.LandingX != null && n.LandingY != null)
				.ToListAsync(cancellationToken)
			: [];

		var points = new List<TacticPoint>(request.Grenades.Count);
		foreach (var (grenade, index) in request.Grenades.OrderBy(g => g.SecondsIntoRound).Select((g, i) => (g, i)))
		{
			var nadeEntryId = request.AddToNadeLibrary ? LinkOrCreateNade(grenade, request, libraryCandidates, now).Id : (Guid?)null;
			points.Add(new TacticPoint
			{
				Id = Guid.NewGuid(),
				TacticId = tactic.Id,
				Order = index,
				X = grenade.LandX,
				Y = grenade.LandY,
				Description = DemoNadeFormatter.PointDescription(grenade.Type, grenade.ThrowerName, grenade.SecondsIntoRound),
				NadeEntryId = nadeEntryId
			});
		}

		dbContext.Tactics.Add(tactic);
		dbContext.TacticPoints.AddRange(points);
		await dbContext.SaveChangesAsync(cancellationToken);

		await realtimeNotifier.NotifyAsync("tactics", cancellationToken);
		if (request.AddToNadeLibrary)
		{
			await realtimeNotifier.NotifyAsync("nades", cancellationToken);
		}

		return new TacticDetailDto(
			tactic.Id,
			tactic.MapName,
			tactic.Side,
			tactic.Name,
			tactic.Economy.ToString(),
			tactic.Note,
			tactic.CreatedByUserId,
			points.Select(p => new TacticPointDto(p.Id, p.Order, p.X, p.Y, p.Description, p.NadeEntryId)).ToList());
	}

	#endregion

	#region Private Methods

	private NadeEntry LinkOrCreateNade(
		ImportedNadeInput grenade, ImportTacticFromDemoCommand request, List<NadeEntry> candidates, DateTime now)
	{
		var existing = DemoNadeFormatter.FindDuplicate(candidates, request.MapName, grenade.Type, grenade.LandX, grenade.LandY);
		if (existing is not null)
		{
			return existing;
		}

		var entry = new NadeEntry
		{
			Id = Guid.NewGuid(),
			MapName = request.MapName,
			Type = grenade.Type,
			Title = DemoNadeFormatter.NadeTitle(grenade.Type, grenade.ThrowerName),
			LandingX = grenade.LandX,
			LandingY = grenade.LandY,
			ThrowX = grenade.ThrowX,
			ThrowY = grenade.ThrowY,
			CreatedByUserId = currentUser.UserId,
			CreatedAtUtc = now
		};

		dbContext.NadeEntries.Add(entry);

		// Added to the candidates too, so the same lineup thrown twice in one import becomes one library entry.
		candidates.Add(entry);
		return entry;
	}

	#endregion
}
