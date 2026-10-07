#region Usings

using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.ExcludePlayer;

/// <summary>Handles <see cref="ExcludePlayerCommand"/>: adds the SteamID64 to the match's exclusion list (idempotent).</summary>
public class ExcludePlayerHandler(IApplicationDbContext dbContext) : IRequestHandler<ExcludePlayerCommand, ErrorOr<Success>>
{
	#region Public Methods

	/// <inheritdoc />
	public async Task<ErrorOr<Success>> Handle(ExcludePlayerCommand request, CancellationToken cancellationToken)
	{
		var analysis = await dbContext.MatchDemoAnalyses.FirstOrDefaultAsync(a => a.MatchResultId == request.MatchResultId, cancellationToken);
		if (analysis is null)
		{
			return MatchAnalysisErrors.TimelineNotFound;
		}

		if (!analysis.ExcludedSteamIds.Contains(request.SteamId64))
		{
			// Reassigned (not Add) so EF notices the change of the array column.
			analysis.ExcludedSteamIds = [.. analysis.ExcludedSteamIds, request.SteamId64];
			await dbContext.SaveChangesAsync(cancellationToken);
		}

		return Result.Success;
	}

	#endregion
}
