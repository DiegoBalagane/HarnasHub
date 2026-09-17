using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Tactics.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Tactics.GetTacticDetail;

/// <summary>Handles <see cref="GetTacticDetailQuery"/>.</summary>
public class GetTacticDetailHandler(IApplicationDbContext dbContext)
	: IRequestHandler<GetTacticDetailQuery, ErrorOr<TacticDetailDto>>
{
	#region Public Methods

	public async Task<ErrorOr<TacticDetailDto>> Handle(GetTacticDetailQuery request, CancellationToken cancellationToken)
	{
		var tactic = await dbContext.Tactics
			.Include(t => t.Points)
			.FirstOrDefaultAsync(t => t.Id == request.TacticId, cancellationToken);

		if (tactic is null)
		{
			return TacticErrors.NotFound;
		}

		return new TacticDetailDto(
			tactic.Id,
			tactic.MapName,
			tactic.Side,
			tactic.Name,
			tactic.Economy.ToString(),
			tactic.Note,
			tactic.CreatedByUserId,
			tactic.Points
				.OrderBy(p => p.Order)
				.Select(p => new TacticPointDto(p.Id, p.Order, p.X, p.Y, p.Description, p.NadeEntryId))
				.ToList());
	}

	#endregion
}
