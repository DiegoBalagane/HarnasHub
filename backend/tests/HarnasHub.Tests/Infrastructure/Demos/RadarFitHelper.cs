#region Usings

using System.Globalization;
using HarnasHub.Application.Abstractions;
using HarnasHub.Infrastructure.Demos;
using Xunit.Abstractions;

#endregion

namespace HarnasHub.Tests.Infrastructure.Demos;

/// <summary>Dev-only helper for re-checking a <c>MapCalibration.RadarImageCrops</c> entry against a real demo: set
/// <c>HARNASHUB_RADAR_FIT_DEMO</c> to a local .dem path and run
/// <c>dotnet test --filter RadarFitHelper --logger "console;verbosity=detailed"</c> — it prints the extents of every sampled
/// player position in world units and in overview fractions (the value a crop is expressed in). Without the variable it
/// does nothing, so it never affects CI.</summary>
public class RadarFitHelper(ITestOutputHelper output)
{
	#region Public Methods

	/// <summary>Prints position extents of the demo named by the environment variable (no-op when unset).</summary>
	[Fact]
	public async Task Print_position_extents_of_a_local_demo()
	{
		var path = Environment.GetEnvironmentVariable("HARNASHUB_RADAR_FIT_DEMO");
		if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
		{
			return;
		}

		await using var stream = File.OpenRead(path);
		var timeline = await new DemoFileParser().ParseAsync(stream, new DemoParseOptions(DemoCollectors.Rounds | DemoCollectors.Positions), CancellationToken.None);

		var world = timeline.Positions
			.SelectMany(t => Enumerable.Range(0, t.Health.Count).Select(i => (X: t.World[i * 3], Y: t.World[i * 3 + 1])))
			.ToList();
		output.WriteLine($"Mapa: {timeline.RawMapName} ({timeline.MapName}), próbek: {world.Count}");
		if (world.Count == 0 || timeline.MapName is not { } map)
		{
			return;
		}

		output.WriteLine(Format("Świat X", world.Select(p => (float)p.X)));
		output.WriteLine(Format("Świat Y", world.Select(p => (float)p.Y)));

		// For a map without a crop entry these are plain overview fractions — the numbers a new crop is fitted from:
		// Left/Top = where the image's playable area starts, Width/Height = how much of the overview the image spans.
		var radar = world.Select(p => MapCalibration.ToRadarFraction(map, p.X, p.Y)).OfType<(float X, float Y)>().ToList();
		output.WriteLine(Format("Radar X", radar.Select(p => p.X)));
		output.WriteLine(Format("Radar Y", radar.Select(p => p.Y)));
	}

	#endregion

	#region Private Methods

	private static string Format(string label, IEnumerable<float> values)
	{
		var sorted = values.Order().ToList();
		float At(double q) => sorted[(int)Math.Clamp(Math.Round(q * (sorted.Count - 1)), 0, sorted.Count - 1)];
		return string.Create(CultureInfo.InvariantCulture,
			$"{label}: min {sorted[0]:0.####}, p1 {At(0.01):0.####}, p99 {At(0.99):0.####}, max {sorted[^1]:0.####}");
	}

	#endregion
}
