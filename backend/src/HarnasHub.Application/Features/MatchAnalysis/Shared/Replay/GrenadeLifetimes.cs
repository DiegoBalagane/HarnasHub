#region Usings

using HarnasHub.Application.Abstractions;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.Shared.Replay;

/// <summary>When a grenade goes off and how long its effect stays on the map. Detonation comes from the timeline when the
/// parser recorded it; otherwise (and for every effect length) CS2's nominal values are used — good enough to draw a smoke
/// or a fire for roughly the right stretch of a replay, not a measurement.</summary>
public static class GrenadeLifetimes
{
	#region Public Methods

	/// <summary>Typical time from throw to detonation when the event wasn't recorded (smokes pop once they stop rolling).</summary>
	public static float DefaultFuseSeconds(DemoGrenadeType type) => type switch
	{
		DemoGrenadeType.Smoke => 2.5f,
		DemoGrenadeType.Flash or DemoGrenadeType.HighExplosive => 1.6f,
		DemoGrenadeType.Molotov or DemoGrenadeType.Incendiary => 1.8f,
		DemoGrenadeType.Decoy => 2f,
		_ => 2f
	};

	/// <summary>How long the effect is drawn after detonation: a smoke ~18 s, a fire ~7 s, a decoy ~15 s, a flash/HE a blink.</summary>
	public static float EffectSeconds(DemoGrenadeType type) => type switch
	{
		DemoGrenadeType.Smoke => 18f,
		DemoGrenadeType.Molotov or DemoGrenadeType.Incendiary => 7f,
		DemoGrenadeType.Decoy => 15f,
		_ => 0.6f
	};

	/// <summary>Detonation and effect end (both seconds into the round, the end capped at <paramref name="roundEndSecond"/>)
	/// and whether the detonation is an approximation.</summary>
	public static (float Detonate, float End, bool Approximate) Resolve(DemoGrenade grenade, float roundEndSecond)
	{
		var approximate = grenade.DetonationSecond is null || grenade.DetonationSecond < grenade.SecondsIntoRound;
		var detonate = approximate ? grenade.SecondsIntoRound + DefaultFuseSeconds(grenade.Type) : grenade.DetonationSecond!.Value;
		var end = Math.Min(detonate + EffectSeconds(grenade.Type), Math.Max(roundEndSecond, detonate));
		return (detonate, end, approximate);
	}

	#endregion
}
