#region Usings

using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.IncludePlayer;

/// <summary>Handles <see cref="IncludePlayerCommand"/>: removes the SteamID64 from the match's exclusion list (idempotent).</summary>
public class IncludePlayerHandler(IApplicationDbContext dbContext) : IRequestHandler<IncludePlayerCommand, ErrorOr<Success>>
{
	#region Public Methods

	/// <inheritdoc />
	public async Task<ErrorOr<Success>> Handle(IncludePlayerCommand request, CancellationToken cancellationToken)
	{
		var analysis = await dbContext.MatchDemoAnalyses.FirstOrDefaultAsync(a => a.MatchResultId == request.MatchResultId, cancellationToken);
		if (analysis is null)
		{
			return MatchAnalysisErrors.TimelineNotFound;
		}

		if (analysis.ExcludedSteamIds.Contains(request.SteamId64))
		{
			analysis.ExcludedSteamIds = analysis.ExcludedSteamIds.Where(id => id != request.SteamId64).ToList();
			await dbContext.SaveChangesAsync(cancellationToken);
		}

		return Result.Success;
	}

	#endregion
}
