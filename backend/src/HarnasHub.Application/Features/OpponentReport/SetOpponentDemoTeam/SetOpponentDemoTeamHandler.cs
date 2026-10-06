#region Usings

using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Application.Features.OpponentReport.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.SetOpponentDemoTeam;

/// <summary>Handles <see cref="SetOpponentDemoTeamCommand"/>: reads the stored timeline back and re-extracts the tendency
/// facts for the chosen team — no re-upload of the demo needed.</summary>
public class SetOpponentDemoTeamHandler(
	IApplicationDbContext dbContext,
	IFileStorage fileStorage,
	ILogger<SetOpponentDemoTeamHandler> logger) : IRequestHandler<SetOpponentDemoTeamCommand, ErrorOr<OpponentDemoDto>>
{
	#region Public Methods

	/// <inheritdoc />
	public async Task<ErrorOr<OpponentDemoDto>> Handle(SetOpponentDemoTeamCommand request, CancellationToken cancellationToken)
	{
		var analysis = await dbContext.OpponentDemoAnalyses.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
		if (analysis is null)
		{
			return OpponentDemoErrors.NotFound;
		}

		if (!fileStorage.IsConfigured)
		{
			return OpponentDemoErrors.StorageNotConfigured;
		}

		StoredDemoTimeline stored;
		try
		{
			stored = await MatchTimelineStorage.LoadAsync(fileStorage, analysis.TimelineObjectKey, cancellationToken);
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			logger.LogWarning(ex, "Nie udało się wczytać osi czasu demki przeciwnika {Id}", analysis.Id);
			return OpponentDemoErrors.TimelineUnavailable;
		}

		OpponentDemoProcessor.ApplyTeam(analysis, stored.Timeline, request.Team);
		await dbContext.SaveChangesAsync(cancellationToken);
		return await OpponentPlayerNames.ResolveDemoAsync(dbContext, OpponentDemoProcessor.ToDto(analysis), cancellationToken);
	}

	#endregion
}
