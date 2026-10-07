#region Usings

using System.Globalization;
using HarnasHub.Application.Abstractions;
using HarnasHub.Infrastructure.Demos;
using Xunit;

#endregion

namespace HarnasHub.Tests.Infrastructure.Demos;

/// <summary>Dev-only helper for fitting a radar crop numerically: set <c>HARNASHUB_POSITION_DUMP_DEMO</c> to a .dem path and
/// <c>HARNASHUB_POSITION_DUMP_OUT</c> to an output file, then run <c>dotnet test --filter PositionDumpHelper</c>. It writes
/// every sampled player position as plain overview fractions (no image crop applied) plus the world Z, side and round second ("x;y;z;side;second"), after "#map" and bomb plant lines ("P;x;y;z;site").
/// Without the variables it does nothing, so it never affects CI.</summary>
public class PositionDumpHelper
{
	#region Public Methods

	/// <summary>Dumps the demo's sampled positions in overview space (no-op when the variables are unset).</summary>
	[Fact]
	public async Task Dump_overview_positions_of_a_local_demo()
	{
		var path = Environment.GetEnvironmentVariable("HARNASHUB_POSITION_DUMP_DEMO");
		var output = Environment.GetEnvironmentVariable("HARNASHUB_POSITION_DUMP_OUT");
		if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(output) || !File.Exists(path))
		{
			return;
		}

		await using var stream = File.OpenRead(path);
		var timeline = await new DemoFileParser().ParseAsync(stream, new DemoParseOptions(DemoCollectors.Rounds | DemoCollectors.Positions), CancellationToken.None);
		if (timeline.MapName is not { } map || MapCalibration.Overview(map) is not { } overview)
		{
			return;
		}

		var samples = timeline.Positions
			.SelectMany(track => Enumerable.Range(0, track.Health.Count).Select(i => (X: track.World[i * 3], Y: track.World[i * 3 + 1], Z: track.World[i * 3 + 2], track.Side, Second: track.StartSecond + i)))
			.Select(p =>
			{
				var (x, y) = overview(p.X, p.Y);
				return string.Create(CultureInfo.InvariantCulture, $"{x:0.#####};{y:0.#####};{p.Z};{p.Side};{p.Second}");
			});
		var plants = timeline.Rounds
			.Where(r => r.BombPlant?.Position is not null)
			.Select(r =>
			{
				var position = r.BombPlant!.Position!;
				var (x, y) = overview(position.WorldX, position.WorldY);
				return string.Create(CultureInfo.InvariantCulture, $"P;{x:0.#####};{y:0.#####};{position.WorldZ};{r.BombPlant.Site}");
			});

		await File.WriteAllLinesAsync(output, [$"#{map}", .. plants, .. samples]);
	}

	#endregion
}
