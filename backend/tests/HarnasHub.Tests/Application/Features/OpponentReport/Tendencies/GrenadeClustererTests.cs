#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentReport.Tendencies;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.OpponentReport.Tendencies;

public class GrenadeClustererTests
{
	#region Public Methods

	[Fact]
	public void Should_group_close_landings_of_the_same_type_and_count_distinct_rounds()
	{
		var clusters = GrenadeClusterer.Cluster(
		[
			new ClusterInput(DemoGrenadeType.Smoke, 0.50f, 0.50f, 0, 1),
			new ClusterInput(DemoGrenadeType.Smoke, 0.51f, 0.50f, 0, 2),
			new ClusterInput(DemoGrenadeType.Smoke, 0.50f, 0.51f, 1, 2),
			new ClusterInput(DemoGrenadeType.Smoke, 0.50f, 0.505f, 1, 2),
			new ClusterInput(DemoGrenadeType.Flash, 0.50f, 0.50f, 0, 1),
			new ClusterInput(DemoGrenadeType.Smoke, 0.90f, 0.90f, 0, 3)
		], minRounds: 1);

		var smoke = clusters[0];
		Assert.Equal(DemoGrenadeType.Smoke, smoke.Type);
		Assert.Equal(4, smoke.Throws);
		Assert.Equal(3, smoke.Rounds);
		Assert.InRange(smoke.X, 0.50f, 0.51f);
		Assert.Equal(3, clusters.Count);
	}

	[Fact]
	public void Should_drop_clusters_seen_in_fewer_rounds_than_required()
	{
		var clusters = GrenadeClusterer.Cluster(
		[
			new ClusterInput(DemoGrenadeType.Smoke, 0.1f, 0.1f, 0, 1),
			new ClusterInput(DemoGrenadeType.Smoke, 0.1f, 0.1f, 0, 1)
		]);

		Assert.Empty(clusters);
	}

	[Fact]
	public void Should_keep_landings_further_apart_than_the_radius_separate()
	{
		var clusters = GrenadeClusterer.Cluster(
		[
			new ClusterInput(DemoGrenadeType.Molotov, 0.1f, 0.1f, 0, 1),
			new ClusterInput(DemoGrenadeType.Molotov, 0.1f + GrenadeClusterer.Radius * 1.5f, 0.1f, 0, 2)
		], minRounds: 1);

		Assert.Equal(2, clusters.Count);
	}

	#endregion
}
