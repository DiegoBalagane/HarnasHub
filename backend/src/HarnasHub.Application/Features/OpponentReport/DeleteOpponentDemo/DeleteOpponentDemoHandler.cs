#region Usings

using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentReport.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.DeleteOpponentDemo;

/// <summary>Handles <see cref="DeleteOpponentDemoCommand"/>: removes the row and (best-effort) its timeline object.</summary>
public class DeleteOpponentDemoHandler(
	IApplicationDbContext dbContext,
	IFileStorage fileStorage,
	ILogger<DeleteOpponentDemoHandler> logger) : IRequestHandler<DeleteOpponentDemoCommand, ErrorOr<Deleted>>
{
	#region Public Methods

	/// <inheritdoc />
	public async Task<ErrorOr<Deleted>> Handle(DeleteOpponentDemoCommand request, CancellationToken cancellationToken)
	{
		var analysis = await dbContext.OpponentDemoAnalyses.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
		if (analysis is null)
		{
			return OpponentDemoErrors.NotFound;
		}

		if (fileStorage.IsConfigured)
		{
			try
			{
				await fileStorage.DeleteAsync(analysis.TimelineObjectKey, cancellationToken);
			}
			catch (Exception ex) when (ex is not OperationCanceledException)
			{
				// The row goes anyway: an orphaned timeline costs a few hundred KB, a row pointing nowhere breaks nothing either.
				logger.LogWarning(ex, "Nie udało się usunąć osi czasu demki przeciwnika (ObjectKey={ObjectKey})", analysis.TimelineObjectKey);
			}
		}

		dbContext.OpponentDemoAnalyses.Remove(analysis);
		await dbContext.SaveChangesAsync(cancellationToken);
		return Result.Deleted;
	}

	#endregion
}
