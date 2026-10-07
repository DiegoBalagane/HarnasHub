#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Common.Maps;
using HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.Shared;

/// <summary>Pure projection of a stored <see cref="DemoTimeline"/> into the match page's <see cref="MatchTimelineDto"/>:
/// resolves our side per round, the running score, buy types, bomb site (game-reported, else map zones) and kill zones.</summary>
public static class MatchTimelineMapper
{
	#region Public Methods

	/// <summary>Maps <paramref name="stored"/> with <paramref name="ourTeamSteamIds"/> as "us"; an empty set falls back to
	/// the team that started on T and reports <see cref="MatchTimelineDto.OurTeamResolved"/> false.</summary>
	public static MatchTimelineDto Map(StoredDemoTimeline stored, IReadOnlyCollection<long> ourTeamSteamIds)
	{
		var timeline = RoundParticipants.Filter(stored.Timeline);
		var rounds = timeline.Rounds.OrderBy(r => r.Number).ToList();
		var resolved = ourTeamSteamIds.Count > 0;
		var ourSet = resolved
			? ourTeamSteamIds.ToHashSet()
			: (rounds.FirstOrDefault()?.TerroristSteamIds ?? []).ToHashSet();

		var economyByRound = timeline.Economy.ToDictionary(e => e.RoundNumber);
		var killsByRound = timeline.Kills.ToLookup(k => k.RoundNumber);

		var ourScore = 0;
		var opponentScore = 0;
		var mapped = new List<MatchRoundDto>(rounds.Count);

		foreach (var round in rounds)
		{
			var ourSide = TimelineTeamResolver.OurSide(round, ourSet);
			bool? weWon = round.WinnerSide is { } winner && ourSide is { } side ? winner == side : null;

			if (weWon == true)
			{
				ourScore++;
			}
			else if (weWon == false)
			{
				opponentScore++;
			}

			economyByRound.TryGetValue(round.Number, out var economy);
			var opponentSide = ourSide is { } s ? Opposite(s) : (MapSide?)null;

			mapped.Add(new MatchRoundDto(
				round.Number,
				round.WinnerSide,
				ourSide,
				weWon,
				ourScore,
				opponentScore,
				round.EndReason,
				round.FreezeEndTime is { } freezeEnd ? Math.Max(0f, round.EndTime - freezeEnd) : null,
				TeamEconomy(round.Number, economy, ourSide),
				TeamEconomy(round.Number, economy, opponentSide),
				Bomb(round, timeline.MapName),
				killsByRound[round.Number].OrderBy(k => k.SecondsIntoRound).Select(k => Kill(k, ourSide, timeline.MapName)).ToList()));
		}

		var hasZones = timeline.MapName is { } map && MapZones.HasZones(map);
		return new MatchTimelineDto(timeline.MapName?.ToString(), hasZones, resolved, stored.ParserVersion, mapped);
	}

	/// <summary>Resolves a bomb event's site: the game's own value when present, otherwise the bombsite zone it lies in.</summary>
	public static DemoBombSite? ResolveSite(DemoBombEvent bombEvent, MapName? map)
	{
		if (bombEvent.Site is { } site)
		{
			return site;
		}

		return MapZones.Find(map, bombEvent.Position?.RadarX, bombEvent.Position?.RadarY)?.Kind switch
		{
			MapZoneKind.BombsiteA => DemoBombSite.A,
			MapZoneKind.BombsiteB => DemoBombSite.B,
			_ => null
		};
	}

	#endregion

	#region Private Methods

	private static MapSide Opposite(MapSide side) => side == MapSide.T ? MapSide.CT : MapSide.T;

	private static MatchTeamEconomyDto? TeamEconomy(int roundNumber, DemoRoundEconomy? economy, MapSide? side)
	{
		if (economy is null || side is not { } teamSide)
		{
			return null;
		}

		var players = economy.Players.Where(p => p.Side == teamSide).ToList();
		if (players.Count == 0)
		{
			return null;
		}

		var equipment = players.Sum(p => p.EquipmentValue);
		return new MatchTeamEconomyDto(
			teamSide,
			equipment,
			players.Sum(p => p.Money),
			players.Count,
			BuyTypeClassifier.Classify(roundNumber, equipment, players.Count));
	}

	private static MatchBombDto? Bomb(DemoTimelineRound round, MapName? map)
	{
		if (round.BombPlant is not { } plant)
		{
			return null;
		}

		return new MatchBombDto(
			ResolveSite(plant, map),
			plant.SecondsIntoRound,
			plant.PlayerName,
			round.BombDefuse is not null,
			round.BombDefuse?.SecondsIntoRound,
			round.BombDefuse?.PlayerName);
	}

	private static MatchKillDto Kill(DemoKill kill, MapSide? ourSide, MapName? map)
	{
		bool? byUs = kill.Killer?.Side is { } killerSide && !kill.IsTeamKill && ourSide is { } side ? killerSide == side : null;
		var killerPosition = kill.Killer?.Position;
		var victimPosition = kill.Victim.Position;

		return new MatchKillDto(
			kill.SecondsIntoRound,
			kill.Killer?.Name,
			kill.Killer?.SteamId64.ToString(),
			kill.Killer?.Side,
			kill.Victim.Name,
			kill.Victim.SteamId64.ToString(),
			kill.Victim.Side,
			kill.Assister?.Name,
			kill.Weapon,
			kill.Headshot,
			kill.Wallbang,
			kill.ThroughSmoke,
			kill.NoScope,
			kill.AttackerBlind,
			kill.IsOpening,
			kill.IsTeamKill,
			byUs,
			MapZones.Find(map, killerPosition?.RadarX, killerPosition?.RadarY)?.Name,
			MapZones.Find(map, victimPosition?.RadarX, victimPosition?.RadarY)?.Name,
			killerPosition?.RadarX,
			killerPosition?.RadarY,
			victimPosition?.RadarX,
			victimPosition?.RadarY);
	}

	#endregion
}
