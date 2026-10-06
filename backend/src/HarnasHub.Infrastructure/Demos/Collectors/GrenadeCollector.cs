#region Usings

using DemoFile;
using DemoFile.Game.Cs;
using HarnasHub.Application.Abstractions;

#endregion

namespace HarnasHub.Infrastructure.Demos.Collectors;

/// <summary>Every grenade thrown in an official round: thrower, side, throw position and time (from the projectile
/// entity's creation) and landing position (from the matching detonate/burn event, else the projectile's last position).</summary>
internal sealed class GrenadeCollector(CsDemoParser demo, DemoRoundClock clock) : IDemoCollector
{
	#region Private Fields

	// A molotov's fire usually starts within a few units of where its projectile was last seen; anything further
	// away is someone else's fire (or a fire from a grenade thrown before the recording started).
	private const float MaxFireMatchDistance = 400f;

	private readonly List<TrackedGrenade> _grenades = [];

	// Keyed by entity index. Entries aren't removed on delete — a detonate event may be processed after the
	// projectile's own delete in the same tick — and are simply overwritten when the engine reuses the index.
	private readonly Dictionary<uint, TrackedGrenade> _byEntityIndex = [];
	private int _nextId = 1;

	#endregion

	#region Public Methods

	/// <inheritdoc />
	public void Subscribe()
	{
		// Subscribed per concrete projectile class rather than on CBaseCSGrenadeProjectile, so each projectile is
		// seen exactly once whether or not the library also raises base-class events for derived entities.
		ref var entities = ref demo.EntityEvents;
		entities.CSmokeGrenadeProjectile.Create += p => OnCreate(p, DemoGrenadeType.Smoke);
		entities.CFlashbangProjectile.Create += p => OnCreate(p, DemoGrenadeType.Flash);
		entities.CHEGrenadeProjectile.Create += p => OnCreate(p, DemoGrenadeType.HighExplosive);
		entities.CDecoyProjectile.Create += p => OnCreate(p, DemoGrenadeType.Decoy);
		entities.CMolotovProjectile.Create += p => OnCreate(p, p.IsIncGrenade ? DemoGrenadeType.Incendiary : DemoGrenadeType.Molotov);

		entities.CSmokeGrenadeProjectile.PostUpdate += OnUpdate;
		entities.CFlashbangProjectile.PostUpdate += OnUpdate;
		entities.CHEGrenadeProjectile.PostUpdate += OnUpdate;
		entities.CDecoyProjectile.PostUpdate += OnUpdate;
		entities.CMolotovProjectile.PostUpdate += OnMolotovUpdate;

		demo.Source1GameEvents.RoundAnnounceMatchStart += _ => OnMatchStart();
		demo.Source1GameEvents.SmokegrenadeDetonate += e => OnDetonate(e.Entityid, e.X, e.Y, e.Z);
		demo.Source1GameEvents.FlashbangDetonate += e => OnDetonate(e.Entityid, e.X, e.Y, e.Z);
		demo.Source1GameEvents.HegrenadeDetonate += e => OnDetonate(e.Entityid, e.X, e.Y, e.Z);
		demo.Source1GameEvents.DecoyStarted += e => OnDetonate(e.Entityid, e.X, e.Y, e.Z);
		demo.Source1GameEvents.InfernoStartburn += e => OnFireStarted(e.X, e.Y, e.Z);
	}

	/// <inheritdoc />
	public void Contribute(DemoTimelineBuilder builder)
	{
		foreach (var grenade in _grenades)
		{
			builder.Grenades.Add(grenade.ToPublic(builder));
		}
	}

	#endregion

	#region Private Methods

	private void OnMatchStart()
	{
		_grenades.Clear();
		_byEntityIndex.Clear();
	}

	private void OnCreate(CBaseCSGrenadeProjectile projectile, DemoGrenadeType type)
	{
		var index = projectile.EntityIndex.Value;
		_byEntityIndex.Remove(index);

		if (!clock.IsOfficialMatchRound)
		{
			return;
		}

		// InitialPosition is where the projectile left the thrower's hand; fall back to the current origin when the
		// field hasn't been networked yet.
		var initial = projectile.InitialPosition;
		var throwWorld = initial is { X: 0, Y: 0, Z: 0 }
			? (projectile.Origin.X, projectile.Origin.Y, projectile.Origin.Z)
			: (initial.X, initial.Y, initial.Z);

		var grenade = new TrackedGrenade(_nextId++, clock.CurrentRoundNumber, type, clock.SecondsIntoRound(), throwWorld);
		ResolveThrower(grenade, projectile);

		_grenades.Add(grenade);
		_byEntityIndex[index] = grenade;
	}

	private void OnMolotovUpdate(CMolotovProjectile projectile)
	{
		if (_byEntityIndex.TryGetValue(projectile.EntityIndex.Value, out var grenade) && projectile.IsIncGrenade)
		{
			grenade.Type = DemoGrenadeType.Incendiary;
		}

		OnUpdate(projectile);
	}

	private void OnUpdate(CBaseCSGrenadeProjectile projectile)
	{
		if (!_byEntityIndex.TryGetValue(projectile.EntityIndex.Value, out var grenade))
		{
			return;
		}

		grenade.LastWorld = (projectile.Origin.X, projectile.Origin.Y, projectile.Origin.Z);

		if (grenade.ThrowerSteamId64 is null)
		{
			ResolveThrower(grenade, projectile);
		}
	}

	private void OnDetonate(int entityId, float x, float y, float z)
	{
		if (_byEntityIndex.TryGetValue((uint)entityId, out var grenade))
		{
			grenade.Detonate(x, y, z, clock.SecondsIntoRound());
		}
	}

	private void OnFireStarted(float x, float y, float z)
	{
		TrackedGrenade.FindFireSource(_grenades, clock.CurrentRoundNumber, x, y, MaxFireMatchDistance)?.Detonate(x, y, z, clock.SecondsIntoRound());
	}

	private static void ResolveThrower(TrackedGrenade grenade, CBaseCSGrenadeProjectile projectile)
	{
		if (projectile.Thrower is not { } pawn || (pawn.OriginalController ?? pawn.Controller) is not { } controller)
		{
			return;
		}

		grenade.ThrowerSteamId64 = (long)controller.SteamID;
		grenade.ThrowerName = controller.PlayerName;
		grenade.ThrowerSide = DemoTeams.Side(controller.CSTeamNum);
	}

	#endregion
}
