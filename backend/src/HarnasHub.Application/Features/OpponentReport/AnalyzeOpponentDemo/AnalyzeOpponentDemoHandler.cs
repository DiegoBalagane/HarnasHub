#region Usings

using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Common.Faceit;
using HarnasHub.Application.Features.OpponentNotes.Shared;
using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.AnalyzeOpponentDemo;

/// <summary>Handles <see cref="AnalyzeOpponentDemoCommand"/>: parses the uploaded demo with position sampling, deletes the
/// .dem, stores the timeline under <c>opponents/{key}/{id}.json.gz</c> and records the analysis — the opponent's team is
/// detected automatically when possible (first from the FACEIT room named by the file, if any), otherwise the response
/// carries both rosters for the coach to pick from.</summary>
public class AnalyzeOpponentDemoHandler(
	IApplicationDbContext dbContext,
	IFileStorage fileStorage,
	IDemoParser demoParser,
	ICurrentUserService currentUser,
	FaceitDemoMatchLookup faceitLookup,
	ILogger<AnalyzeOpponentDemoHandler> logger) : IRequestHandler<AnalyzeOpponentDemoCommand, ErrorOr<OpponentDemoDto>>
{
	#region Public Methods

	/// <inheritdoc />
	public async Task<ErrorOr<OpponentDemoDto>> Handle(AnalyzeOpponentDemoCommand request, CancellationToken cancellationToken)
	{
		if (!fileStorage.IsConfigured)
		{
			return OpponentDemoErrors.StorageNotConfigured;
		}

		var timeline = await OpponentDemoReader.ParseUploadedAsync(fileStorage, demoParser, request.ObjectKey, logger, cancellationToken);
		if (timeline is null)
		{
			return OpponentDemoErrors.InvalidDemoFile;
		}

		try
		{
			var key = OpponentNames.ToKey(request.OpponentName);
			var input = await BuildInputAsync(key, request.FileName, cancellationToken);
			var analysis = await OpponentDemoProcessor.StoreAsync(dbContext, fileStorage, input, timeline, DateTime.UtcNow, cancellationToken);
			await dbContext.SaveChangesAsync(cancellationToken);
			return OpponentDemoProcessor.ToDto(analysis);
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			logger.LogError(ex, "Nie udało się zapisać analizy demki przeciwnika {Opponent}", request.OpponentName);
			return OpponentDemoErrors.TimelineUnavailable;
		}
	}

	#endregion

	#region Private Methods

	/// <summary>The upload's input; a recognised FACEIT room adds the match id/map/date and the opponent faction's SteamIDs.</summary>
	private async Task<OpponentDemoInput> BuildInputAsync(string key, string? fileName, CancellationToken cancellationToken)
	{
		var plain = new OpponentDemoInput(key, OpponentDemoSource.Upload, null, null, null, currentUser.UserId);
		if ((await faceitLookup.FindAsync(fileName, cancellationToken)).Match is not { } found)
		{
			return plain;
		}

		var link = await dbContext.OpponentFaceitLinks.AsNoTracking().FirstOrDefaultAsync(l => l.OpponentKey == key, cancellationToken);
		var opponentIndex = FaceitFactionMatcher.OpponentFactionIndex(
			found.Match.Factions, found.OurFactionIndex, (link?.PlayerIds ?? []).ToHashSet(), key);

		return plain with
		{
			FaceitMatchId = found.Match.MatchId,
			FaceitMapNumber = found.Reference.MapNumber ?? 1,
			PlayedAtUtc = found.Match.StartedAtUtc ?? found.Match.FinishedAtUtc,
			OpponentSteamIdsHint = opponentIndex is { } index ? FaceitFactionMatcher.SteamIds(found.Match.Factions[index]) : null
		};
	}

	#endregion
}
