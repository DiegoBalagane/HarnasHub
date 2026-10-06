using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;

namespace HarnasHub.Tests.Application.Features.OpponentNotes;

/// <summary>Small builders for the three places an opponent name shows up, shared by the opponent list/profile tests.</summary>
internal static class OpponentTestData
{
	#region Public Methods

	/// <summary>A scouting note about <paramref name="opponent"/>.</summary>
	public static OpponentNote Note(string opponent, DateTime createdAtUtc, string content = "Notatka") => new()
	{
		Id = Guid.NewGuid(),
		OpponentName = opponent,
		Content = content,
		CreatedByUserId = Guid.NewGuid(),
		CreatedAtUtc = createdAtUtc
	};

	/// <summary>A logged result against <paramref name="opponent"/>.</summary>
	public static MatchResult Match(string opponent, int ourScore, int opponentScore, DateTime playedAtUtc, string? mapName = null) => new()
	{
		Id = Guid.NewGuid(),
		Opponent = opponent,
		OurScore = ourScore,
		OpponentScore = opponentScore,
		MapName = mapName,
		Category = MatchCategory.Scrimmage,
		PlayedAtUtc = playedAtUtc,
		CreatedByUserId = Guid.NewGuid(),
		CreatedAtUtc = playedAtUtc
	};

	/// <summary>A scheduled match against <paramref name="opponent"/>.</summary>
	public static Event Game(string? opponent, DateTime startsAtUtc) => new()
	{
		Id = Guid.NewGuid(),
		Title = "Mecz",
		Type = EventType.Match,
		StartsAtUtc = startsAtUtc,
		Opponent = opponent,
		CreatedByUserId = Guid.NewGuid(),
		CreatedAtUtc = DateTime.UtcNow
	};

	#endregion
}
