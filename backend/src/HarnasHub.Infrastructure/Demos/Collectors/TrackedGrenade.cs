#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Infrastructure.Demos.Collectors;

/// <summary>Mutable state for one grenade projectile while the demo is being read; turned into a
/// <see cref="DemoGrenade"/> once the read is finished.</summary>
internal sealed class TrackedGrenade(int id, int roundNumber, DemoGrenadeType type, float secondsIntoRound, (float X, float Y, float Z) throwWorld)
{
	#region Public Properties

	public int Id { get; } = id;
	public int RoundNumber { get; } = roundNumber;
	public DemoGrenadeType Type { get; set; } = type;
	public float SecondsIntoRound { get; } = secondsIntoRound;
	public (float X, float Y, float Z) ThrowWorld { get; set; } = throwWorld;
	public long? ThrowerSteamId64 { get; set; }
	public string? ThrowerName { get; set; }
	public MapSide? ThrowerSide { get; set; }
	public (float X, float Y, float Z)? LastWorld { get; set; }
	public (float X, float Y, float Z)? LandingWorld { get; set; }

	/// <summary>True once the landing came from a detonation/burn event rather than the projectile's last seen position;
	/// event positions are authoritative and never overwritten.</summary>
	public bool HasDetonationPosition { get; set; }

	/// <summary>Seconds into the round when the detonation event arrived; null until then (or when the caller didn't know).</summary>
	public float? DetonationSecond { get; set; }

	/// <summary>Whether this is a fire grenade (molotov or incendiary).</summary>
	public bool IsFire => Type is DemoGrenadeType.Molotov or DemoGrenadeType.Incendiary;

	#endregion

	#region Public Methods

	/// <summary>Records an authoritative detonation point (and when it happened, if known), unless one was already recorded.</summary>
	public void Detonate(float x, float y, float z, float? secondsIntoRound = null)
	{
		if (HasDetonationPosition)
		{
			return;
		}

		LandingWorld = (x, y, z);
		HasDetonationPosition = true;
		DetonationSecond = secondsIntoRound;
	}

	/// <summary>Converts to the public timeline record.</summary>
	public DemoGrenade ToPublic(DemoTimelineBuilder builder)
	{
		var landing = LandingWorld ?? LastWorld;
		return new DemoGrenade(
			Id,
			RoundNumber,
			Type,
			ThrowerSteamId64,
			ThrowerName ?? "?",
			ThrowerSide,
			SecondsIntoRound,
			builder.ToPosition(ThrowWorld.X, ThrowWorld.Y, ThrowWorld.Z),
			landing is { } l ? builder.ToPosition(l.X, l.Y, l.Z) : null)
		{
			DetonationSecond = DetonationSecond
		};
	}

	/// <summary>Picks the fire grenade an inferno_startburn belongs to: the not-yet-burnt one of the given round whose
	/// last known position is nearest the fire, within <paramref name="maxDistance"/> world units — the event's entity id
	/// is the inferno's own, not the projectile's, so it can't be linked directly.</summary>
	public static TrackedGrenade? FindFireSource(
		IEnumerable<TrackedGrenade> grenades, int roundNumber, float x, float y, float maxDistance)
	{
		TrackedGrenade? best = null;
		var bestDistance = maxDistance * maxDistance;

		foreach (var grenade in grenades)
		{
			if (!grenade.IsFire || grenade.HasDetonationPosition || grenade.RoundNumber != roundNumber)
			{
				continue;
			}

			var reference = grenade.LandingWorld ?? grenade.LastWorld ?? grenade.ThrowWorld;
			var dx = reference.X - x;
			var dy = reference.Y - y;
			var distance = dx * dx + dy * dy;

			if (distance <= bestDistance)
			{
				bestDistance = distance;
				best = grenade;
			}
		}

		return best;
	}

	#endregion
}
