namespace HarnasHub.Core.Enums;

/// <summary>Where a map stands in the team's pool, as set by a coach or manager.</summary>
public enum MapPoolStatus
{
	/// <summary>Comfort pick — the map the team wants to play.</summary>
	Core = 0,
	/// <summary>Playable, but not a first pick.</summary>
	Playable = 1,
	/// <summary>Being learned — not ready for official games yet.</summary>
	Learning = 2,
	/// <summary>Permanent ban — the team never wants to play it.</summary>
	Ban = 3
}
