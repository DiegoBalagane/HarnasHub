namespace HarnasHub.Core.Enums;

/// <summary>What a map veto step did to the map.</summary>
public enum VetoAction
{
	Ban = 0,
	Pick = 1,
	/// <summary>The map left over after all bans/picks — played without either team choosing it.</summary>
	Decider = 2
}
