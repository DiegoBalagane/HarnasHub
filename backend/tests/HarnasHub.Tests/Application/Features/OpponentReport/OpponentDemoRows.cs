#region Usings

using HarnasHub.Application.Features.OpponentReport.Tendencies;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;

#endregion

namespace HarnasHub.Tests.Application.Features.OpponentReport;

/// <summary>Builds <see cref="OpponentDemoAnalysis"/> rows for the opponent demo handler tests.</summary>
public static class OpponentDemoRows
{
	#region Public Methods

	/// <summary>A row of "team x" on Mirage; resolved (with the given facts) unless <paramref name="facts"/> is null.</summary>
	public static OpponentDemoAnalysis Row(OpponentDemoFacts? facts = null, string opponentKey = "team x", MapName? map = MapName.Mirage) => new()
	{
		Id = Guid.NewGuid(),
		OpponentKey = opponentKey,
		MapName = map,
		Source = OpponentDemoSource.Upload,
		TimelineObjectKey = $"opponents/team-x/{Guid.NewGuid():N}.json.gz",
		RoundsCount = 24,
		TeamASteamIds = [.. OpponentTimelineFactory.Them],
		TeamBSteamIds = [.. OpponentTimelineFactory.Others],
		TeamANames = OpponentTimelineFactory.Them.Select(id => $"p{id}").ToList(),
		TeamBNames = OpponentTimelineFactory.Others.Select(id => $"p{id}").ToList(),
		OpponentSteamIds = facts is null ? [] : [.. OpponentTimelineFactory.Them],
		FactsJson = facts is null ? null : OpponentFactsSerializer.Serialize(facts),
		CreatedAtUtc = DateTime.UtcNow
	};

	/// <summary>Links "team x" to FACEIT players whose SteamIDs are 11 and 12.</summary>
	public static void SeedLink(TestApplicationDbContext dbContext)
	{
		dbContext.OpponentFaceitLinks.Add(new OpponentFaceitLink
		{
			Id = Guid.NewGuid(),
			OpponentKey = "team x",
			DisplayName = "Team X",
			PlayerIds = ["f11", "f12"],
			LinkedAtUtc = DateTime.UtcNow
		});
		dbContext.FaceitPlayers.Add(new FaceitPlayer { Id = "f11", Nickname = "a", SteamId64 = "11" });
		dbContext.FaceitPlayers.Add(new FaceitPlayer { Id = "f12", Nickname = "b", SteamId64 = "12" });
		dbContext.SaveChanges();
	}

	#endregion
}
