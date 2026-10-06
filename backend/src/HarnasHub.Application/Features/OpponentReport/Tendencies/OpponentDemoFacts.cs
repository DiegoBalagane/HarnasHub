#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.Tendencies;

/// <summary>Coarse part of a map a position belongs to — what tendencies are counted in.</summary>
public enum MapArea
{
	A = 0,
	B = 1,
	Mid = 2
}

/// <summary>What the opponent's CT side did once the enemy planted the bomb.</summary>
public enum PostPlantBehaviour
{
	Retake = 0,
	Save = 1,
	AllDead = 2
}

/// <summary>Everything tendencies need from one opponent demo, extracted once when the demo is analysed and stored with
/// the analysis row (small JSON), so the report never has to download and re-read whole timelines. Every value is from the
/// opponent's perspective; <paramref name="Version"/> lets a future extractor change be detected.</summary>
public record OpponentDemoFacts(int Version, List<OpponentRoundFacts> Rounds, List<OpponentPlayerFacts> Players)
{
	/// <summary>Version of the extraction rules; bump when <c>OpponentFactsExtractor</c> changes what it produces.</summary>
	public const int CurrentVersion = 1;
}

/// <summary>One round as the opponent played it; exactly one of <paramref name="T"/>/<paramref name="Ct"/> is set, per
/// <paramref name="Side"/>. <paramref name="Buy"/> is the <c>BuyType</c> name of their loadout (null without economy data).</summary>
public record OpponentRoundFacts(
	int Number,
	MapSide Side,
	bool? Won,
	string? Buy,
	OpponentTRoundFacts? T,
	OpponentCtRoundFacts? Ct);

/// <summary>A radar-fraction point.</summary>
public record FactPoint(float X, float Y);

/// <summary>A T round: the first duel (where/when), the plant (where/when) and the utility thrown up to the execute.</summary>
public record OpponentTRoundFacts(
	MapArea? FirstContactArea,
	float? FirstContactSecond,
	FactPoint? FirstContact,
	MapArea? PlantArea,
	float? PlantSecond,
	List<GrenadeFact> Grenades)
{
	/// <summary>Where the round went: the plant site, else the area of the first duel.</summary>
	public MapArea? Target => PlantArea ?? FirstContactArea;

	/// <summary>When the round was played out: the plant, else the first duel (seconds from freeze end).</summary>
	public float? ExecSecond => PlantSecond ?? FirstContactSecond;
}

/// <summary>One grenade the opponent threw on T side, with its landing point.</summary>
public record GrenadeFact(DemoGrenadeType Type, float X, float Y, float Second);

/// <summary>A CT round: everyone's spot at the setup second, AWP kills, early duels and the reaction to a plant.</summary>
public record OpponentCtRoundFacts(
	List<CtSetupSpot> Setup,
	List<AwpKillFact> AwpKills,
	int EarlyKills,
	int EarlyDeaths,
	PostPlantBehaviour? PostPlant);

/// <summary>Where one CT player stood at the setup second, and whether they had an AWP that round.</summary>
public record CtSetupSpot(float X, float Y, MapArea? Area, bool Awp);

/// <summary>An AWP kill by the opponent's CT side: killer position and time.</summary>
public record AwpKillFact(float X, float Y, MapArea? Area, float Second);

/// <summary>One opponent player's totals within a demo.</summary>
public record OpponentPlayerFacts(
	long SteamId64,
	string Name,
	int Rounds,
	int Kills,
	int OpeningKills,
	int OpeningDeaths,
	int AwpKills,
	int ClutchAttempts,
	int ClutchWins);
