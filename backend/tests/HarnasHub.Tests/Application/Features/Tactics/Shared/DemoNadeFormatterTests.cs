#region Usings

using HarnasHub.Application.Features.Tactics.Shared;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Tests.Application.Features.Tactics.Shared;

/// <summary>Covers the texts generated for imported grenades and the nade-library duplicate detection.</summary>
public class DemoNadeFormatterTests
{
	#region Public Methods

	[Theory]
	[InlineData(14.7f, "0:14")]
	[InlineData(0f, "0:00")]
	[InlineData(65f, "1:05")]
	[InlineData(-3f, "0:00")]
	public void FormatTime_formats_minutes_and_seconds(float seconds, string expected)
	{
		Assert.Equal(expected, DemoNadeFormatter.FormatTime(seconds));
	}

	[Fact]
	public void PointDescription_combines_type_nick_and_time()
	{
		Assert.Equal("Smoke · kacper · 0:14", DemoNadeFormatter.PointDescription(GrenadeType.Smoke, "kacper", 14.2f));
		Assert.Equal("HE · kacper · 1:00", DemoNadeFormatter.PointDescription(GrenadeType.Frag, "kacper", 60f));
	}

	[Fact]
	public void NadeTitle_marks_the_demo_origin_and_fits_the_title_limit()
	{
		Assert.Equal("Flash – kacper (z demki)", DemoNadeFormatter.NadeTitle(GrenadeType.Flash, "kacper"));

		var longTitle = DemoNadeFormatter.NadeTitle(GrenadeType.Molotov, new string('x', 200));
		Assert.Equal(100, longTitle.Length);
		Assert.EndsWith(" (z demki)", longTitle);
	}

	[Fact]
	public void FindDuplicate_returns_the_nearest_entry_of_the_same_map_and_type_within_the_radius()
	{
		var far = Entry(MapName.Mirage, GrenadeType.Smoke, 0.53f, 0.5f);
		var near = Entry(MapName.Mirage, GrenadeType.Smoke, 0.51f, 0.5f);
		var otherType = Entry(MapName.Mirage, GrenadeType.Flash, 0.5f, 0.5f);
		var otherMap = Entry(MapName.Inferno, GrenadeType.Smoke, 0.5f, 0.5f);

		var match = DemoNadeFormatter.FindDuplicate([far, otherType, otherMap, near], MapName.Mirage, GrenadeType.Smoke, 0.5f, 0.5f);

		Assert.Same(near, match);
	}

	[Fact]
	public void FindDuplicate_returns_null_outside_the_radius_or_without_a_pin()
	{
		var tooFar = Entry(MapName.Mirage, GrenadeType.Smoke, 0.53f, 0.5f);
		var noPin = new NadeEntry { MapName = MapName.Mirage, Type = GrenadeType.Smoke };

		Assert.Null(DemoNadeFormatter.FindDuplicate([tooFar, noPin], MapName.Mirage, GrenadeType.Smoke, 0.5f, 0.5f));
	}

	#endregion

	#region Private Methods

	private static NadeEntry Entry(MapName map, GrenadeType type, float x, float y) =>
		new() { Id = Guid.NewGuid(), MapName = map, Type = type, LandingX = x, LandingY = y };

	#endregion
}
