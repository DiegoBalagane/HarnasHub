#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Application.Features.Results.Shared;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

#endregion

namespace HarnasHub.Application.Common.Notifications;

/// <summary>Builds and posts the result / demo review / opponent digest Discord messages; every method is best-effort and never throws, so a Discord or storage hiccup cannot break the request or job that triggered it.</summary>
public class TeamNotifications(
	IDiscordNotifier discord,
	IFrontendLinks links,
	IApplicationDbContext dbContext,
	IFileStorage fileStorage,
	ILogger<TeamNotifications> logger)
{
	#region Public Methods

	/// <summary>Posts "result saved" to the announcements channel — the match schedule channel only lists upcoming matches.</summary>
	public async Task NotifyResultSavedAsync(MatchResult result, CancellationToken cancellationToken)
	{
		try
		{
			var message = MatchResultFormatter.Format(result.Opponent, result.OurScore, result.OpponentScore, result.MapName, links.MatchResult(result.Id));
			await discord.SendAsync(DiscordChannel.Announcements, message, cancellationToken);
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			logger.LogWarning(ex, "Nie udało się wysłać powiadomienia o wyniku meczu {MatchResultId}", result.Id);
		}
	}

	/// <summary>Posts the insight digest of the match's freshly attached timeline to the demo review channel.</summary>
	public async Task NotifyDemoReviewAsync(Guid matchResultId, CancellationToken cancellationToken)
	{
		try
		{
			var result = await dbContext.MatchResults.AsNoTracking().FirstOrDefaultAsync(r => r.Id == matchResultId, cancellationToken);
			if (result is null)
			{
				return;
			}

			var timeline = await MatchTimelineLoader.LoadAsync(dbContext, fileStorage, logger, matchResultId, cancellationToken);
			if (timeline.IsError)
			{
				return;
			}

			var message = DemoReviewFormatter.Format(
				result.Opponent, result.OurScore, result.OpponentScore, result.MapName, MatchInsightRules.Build(timeline.Value), links.MatchResult(result.Id));
			await discord.SendAsync(DiscordChannel.DemoReview, message, cancellationToken);
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			logger.LogWarning(ex, "Nie udało się wysłać podsumowania demki meczu {MatchResultId}", matchResultId);
		}
	}

	#endregion
}
