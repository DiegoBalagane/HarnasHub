#region Usings

using HarnasHub.Application.Common.Time;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Common.Time;

public class TeamTimeTests
{
	#region Public Methods

	[Fact]
	public void Should_show_summer_time_two_hours_ahead_of_utc() =>
		Assert.Equal("07.10 19:00", TeamTime.ShortDateTime(new DateTime(2026, 10, 7, 17, 0, 0, DateTimeKind.Utc)));

	[Fact]
	public void Should_show_winter_time_one_hour_ahead_of_utc() =>
		Assert.Equal("15.01 19:00", TeamTime.ShortDateTime(new DateTime(2027, 1, 15, 18, 0, 0, DateTimeKind.Utc)));

	#endregion
}
