#region Usings

using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.GetMatchAnalysis;

/// <summary>Handles <see cref="GetMatchAnalysisQuery"/>: loads the timeline, the team's nade library for its map and runs <see cref="MatchAnalysisBuilder"/>.</summary>
public class GetMatchAnalysisHandler(IApplicationDbContext dbContext, IFileStorage fileStorage, ILogger<GetMatchAnalysisHandler> logger)
	: IRequestHandler<GetMatchAnalysisQuery, ErrorOr<MatchAnalysisDto>>
{
	#region Public Methods

	/// <inheritdoc />
	public async Task<ErrorOr<MatchAnalysisDto>> Handle(GetMatchAnalysisQuery request, CancellationToken cancellationToken)
	{
		var loaded = await MatchTimelineLoader.LoadRawAsync(dbContext, fileStorage, logger, request.MatchResultId, cancellationToken);
		if (loaded.IsError)
		{
			return loaded.Errors;
		}

		var context = AnalysisContext.Create(loaded.Value.Stored.Timeline, loaded.Value.OurTeam);

		IReadOnlyList<LibraryNade> library = [];
		if (context.Timeline.MapName is { } map)
		{
			library = await dbContext.NadeEntries.AsNoTracking()
				.Where(n => n.MapName == map && n.LandingX != null && n.LandingY != null)
				.Select(n => new LibraryNade(n.Id, n.Type, n.Title, n.LandingX!.Value, n.LandingY!.Value))
				.ToListAsync(cancellationToken);
		}

		return MatchAnalysisBuilder.Build(context, library);
	}

	#endregion
}
