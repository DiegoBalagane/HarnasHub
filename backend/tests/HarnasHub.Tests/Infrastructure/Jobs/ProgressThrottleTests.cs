#region Usings

using HarnasHub.Infrastructure.Jobs;
using Xunit;

#endregion

namespace HarnasHub.Tests.Infrastructure.Jobs;

public class ProgressThrottleTests
{
	#region Private Fields

	private static readonly DateTimeOffset Start = new(2026, 10, 6, 12, 0, 0, TimeSpan.Zero);

	#endregion

	#region Public Methods

	[Fact]
	public void Should_publish_the_first_update_and_every_stage_change_immediately()
	{
		var throttle = new ProgressThrottle(TimeSpan.FromSeconds(1));

		Assert.True(throttle.ShouldPublish(5, "Analiza demki", Start));
		Assert.True(throttle.ShouldPublish(5, "Zapisywanie wyników", Start.AddMilliseconds(10)));
	}

	[Fact]
	public void Should_hold_back_higher_percentages_until_the_interval_passed()
	{
		var throttle = new ProgressThrottle(TimeSpan.FromSeconds(1));
		throttle.ShouldPublish(10, "Analiza demki", Start);

		Assert.False(throttle.ShouldPublish(20, "Analiza demki", Start.AddMilliseconds(500)));
		Assert.True(throttle.ShouldPublish(30, "Analiza demki", Start.AddSeconds(1)));
	}

	[Fact]
	public void Should_never_publish_the_same_or_a_lower_percentage_of_the_same_stage()
	{
		var throttle = new ProgressThrottle(TimeSpan.FromSeconds(1));
		throttle.ShouldPublish(40, "Analiza demki", Start);

		Assert.False(throttle.ShouldPublish(40, "Analiza demki", Start.AddSeconds(5)));
		Assert.False(throttle.ShouldPublish(30, "Analiza demki", Start.AddSeconds(5)));
	}

	#endregion
}
