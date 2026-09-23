#region Usings

using HarnasHub.Core.Enums;
using HarnasHub.Infrastructure.Demos;

#endregion

namespace HarnasHub.Tests.Infrastructure.Demos;

/// <summary>Covers the world-to-radar conversion, including the per-map correction for radar images that are a crop
/// of the official overview rather than the whole thing.</summary>
public class MapCalibrationTests
{
	#region Public Methods

	[Theory]
	[InlineData("de_ancient", MapName.Ancient)]
	[InlineData("DE_MIRAGE", MapName.Mirage)]
	[InlineData("nuke", MapName.Nuke)]
	public void ParseMapName_RecognisesPoolMaps_CaseInsensitively(string raw, MapName expected)
	{
		Assert.Equal(expected, MapCalibration.ParseMapName(raw));
	}

	[Fact]
	public void ParseMapName_ReturnsNull_ForMapOutsidePool()
	{
		Assert.Null(MapCalibration.ParseMapName("de_vertigo"));
	}

	[Fact]
	public void ToRadarFraction_UsesPlainOverviewSpace_ForMapWhoseImageIsTheOverview()
	{
		// Cache ships the untouched 1024x1024 overview, so its pos_x/pos_y corner is the image's top-left corner.
		var corner = MapCalibration.ToRadarFraction(MapName.Cache, -2000, 3250);
		var centre = MapCalibration.ToRadarFraction(MapName.Cache, -2000 + (512 * 5.5f), 3250 - (512 * 5.5f));

		Assert.Equal((0f, 0f), corner);
		Assert.Equal((0.5f, 0.5f), centre);
	}

	[Fact]
	public void ToRadarFraction_RescalesIntoTheCroppedImage_ForMapWhoseImageIsACrop()
	{
		// ancient.webp only shows the sub-rectangle of the overview starting at (0.0785, 0.034) and 0.820 x 0.9325
		// in size, so the overview's own centre sits slightly right of, and slightly above, the image's centre.
		var centre = MapCalibration.ToRadarFraction(MapName.Ancient, -2953 + (512 * 5f), 2164 - (512 * 5f));

		Assert.NotNull(centre);
		Assert.Equal((0.5f - 0.0785f) / 0.820f, centre!.Value.X, 4);
		Assert.Equal((0.5f - 0.034f) / 0.9325f, centre.Value.Y, 4);
	}

	[Fact]
	public void ToRadarFraction_ClampsPositionsOutsideTheImage()
	{
		var farNegative = MapCalibration.ToRadarFraction(MapName.Ancient, -99_000, 99_000);
		var farPositive = MapCalibration.ToRadarFraction(MapName.Ancient, 99_000, -99_000);

		Assert.Equal((0f, 0f), farNegative);
		Assert.Equal((1f, 1f), farPositive);
	}

	#endregion
}
