#region Usings

using HarnasHub.Application.Abstractions;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.Tendencies;

/// <summary>A grenade landing tagged with the round it belongs to (demo index + round number), the clustering input.</summary>
public record ClusterInput(DemoGrenadeType Type, float X, float Y, int DemoIndex, int RoundNumber);

/// <summary>A group of same-type grenades landing within <see cref="GrenadeClusterer.Radius"/> of each other: centre,
/// number of throws and of distinct rounds it appeared in.</summary>
public record GrenadeCluster(DemoGrenadeType Type, float X, float Y, int Throws, int Rounds);

/// <summary>Pure leader clustering of grenade landings: each landing joins the nearest existing cluster of the same type
/// whose centre is within <see cref="Radius"/> (radar fraction, ≈ 2% of the radar), otherwise starts a new one; centres
/// are running means. Deterministic for a given input order, and cheap enough for hundreds of grenades.</summary>
public static class GrenadeClusterer
{
	#region Public Fields

	/// <summary>Maximum distance (radar fraction) from a cluster centre for a landing to join it.</summary>
	public const float Radius = 0.02f;

	#endregion

	#region Private Types

	private sealed class Builder(DemoGrenadeType type, float x, float y)
	{
		public DemoGrenadeType Type { get; } = type;
		public float X { get; private set; } = x;
		public float Y { get; private set; } = y;
		public int Throws { get; private set; }
		public HashSet<(int, int)> Rounds { get; } = [];

		public void Add(ClusterInput input)
		{
			Throws++;
			X += (input.X - X) / Throws;
			Y += (input.Y - Y) / Throws;
			Rounds.Add((input.DemoIndex, input.RoundNumber));
		}
	}

	#endregion

	#region Public Methods

	/// <summary>Clusters with at least <paramref name="minRounds"/> distinct rounds, most frequent first.</summary>
	public static List<GrenadeCluster> Cluster(IEnumerable<ClusterInput> landings, int minRounds = 2)
	{
		var clusters = new List<Builder>();

		foreach (var landing in landings)
		{
			var nearest = clusters
				.Where(c => c.Type == landing.Type)
				.Select(c => (Cluster: c, Distance: Distance(c.X, c.Y, landing.X, landing.Y)))
				.Where(c => c.Distance <= Radius)
				.OrderBy(c => c.Distance)
				.Select(c => c.Cluster)
				.FirstOrDefault();

			if (nearest is null)
			{
				nearest = new Builder(landing.Type, landing.X, landing.Y);
				clusters.Add(nearest);
			}

			nearest.Add(landing);
		}

		return clusters
			.Where(c => c.Rounds.Count >= minRounds)
			.Select(c => new GrenadeCluster(c.Type, c.X, c.Y, c.Throws, c.Rounds.Count))
			.OrderByDescending(c => c.Rounds)
			.ThenByDescending(c => c.Throws)
			.ToList();
	}

	#endregion

	#region Private Methods

	private static float Distance(float x1, float y1, float x2, float y2) =>
		MathF.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));

	#endregion
}
