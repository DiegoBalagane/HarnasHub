using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentManagement.Shared;
using HarnasHub.Application.Features.OpponentNotes.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.OpponentManagement.GetOpponentDeletePreview;

/// <summary>Handles <see cref="GetOpponentDeletePreviewQuery"/> by counting everything stored under the opponent key.</summary>
public class GetOpponentDeletePreviewHandler(IApplicationDbContext dbContext)
	: IRequestHandler<GetOpponentDeletePreviewQuery, ErrorOr<OpponentDeletePreviewDto>>
{
	#region Public Methods

	public async Task<ErrorOr<OpponentDeletePreviewDto>> Handle(GetOpponentDeletePreviewQuery request, CancellationToken cancellationToken)
	{
		var key = OpponentNames.ToKey(request.Name);

		return new OpponentDeletePreviewDto(
			await dbContext.OpponentNotes.CountAsync(n => n.OpponentName.Trim().ToLower() == key, cancellationToken),
			await dbContext.OpponentDemoAnalyses.CountAsync(a => a.OpponentKey == key, cancellationToken),
			await dbContext.OpponentFaceitLinks.AnyAsync(l => l.OpponentKey == key, cancellationToken),
			await dbContext.OpponentReportSnapshots.AnyAsync(s => s.OpponentKey == key, cancellationToken),
			await dbContext.MatchResults.CountAsync(m => m.Opponent.Trim().ToLower() == key, cancellationToken),
			await dbContext.Events.CountAsync(e => e.Opponent != null && e.Opponent.Trim().ToLower() == key, cancellationToken),
			await dbContext.HiddenOpponents.AnyAsync(h => h.OpponentKey == key, cancellationToken));
	}

	#endregion
}
