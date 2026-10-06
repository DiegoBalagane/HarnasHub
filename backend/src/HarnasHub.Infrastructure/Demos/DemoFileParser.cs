#region Usings

using DemoFile;
using HarnasHub.Application.Abstractions;
using HarnasHub.Infrastructure.Demos.Collectors;

#endregion

namespace HarnasHub.Infrastructure.Demos;

/// <summary>Implements <see cref="IDemoParser"/> against the DemoFile.Net library (github.com/saul/demofile-net) — the only
/// actively maintained C# parser for CS2's Source 2 demo format (the older CS:GO-era parsers don't read it). One read of
/// the file feeds every enabled <see cref="IDemoCollector"/>; this class only wires them up and assembles the result.</summary>
public class DemoFileParser : IDemoParser
{
	#region Public Methods

	/// <inheritdoc />
	public async Task<DemoParseResult> ParseAsync(Stream demoStream, CancellationToken cancellationToken)
	{
		var timeline = await ParseAsync(demoStream, DemoParseOptions.StatsOnly, cancellationToken);
		return timeline.Stats!;
	}

	/// <inheritdoc />
	public async Task<DemoTimeline> ParseAsync(Stream demoStream, DemoParseOptions options, CancellationToken cancellationToken)
	{
		var demo = new CsDemoParser();

		// The clock goes first: collectors read its round latch from their own handlers of the same events.
		var clock = new DemoRoundClock(demo);
		clock.Subscribe();

		var collectors = CreateCollectors(demo, clock, options);
		foreach (var collector in collectors)
		{
			collector.Subscribe();
		}

		var reader = DemoFileReader.Create(demo, demoStream);
		await reader.ReadAllAsync(cancellationToken);

		// game_newmap only fires on a mid-session map *change*, never for the map a recording starts on — which
		// is every standalone demo — so the server info packet (present from the start) is the reliable source.
		var rawMapName = demo.ServerInfo?.MapName;
		var mapName = rawMapName is null ? null : MapCalibration.ParseMapName(rawMapName);

		var builder = new DemoTimelineBuilder(rawMapName, mapName);
		foreach (var collector in collectors)
		{
			collector.Contribute(builder);
		}

		return builder.Build();
	}

	#endregion

	#region Private Methods

	private static List<IDemoCollector> CreateCollectors(CsDemoParser demo, DemoRoundClock clock, DemoParseOptions options)
	{
		var collectors = new List<IDemoCollector>();

		if (options.Includes(DemoCollectors.Stats))
		{
			collectors.Add(new StatsCollector(demo, clock));
		}

		if (options.Includes(DemoCollectors.Rounds))
		{
			collectors.Add(new RoundCollector(demo, clock));
		}

		if (options.Includes(DemoCollectors.Economy))
		{
			collectors.Add(new EconomyCollector(demo, clock));
		}

		if (options.Includes(DemoCollectors.Kills))
		{
			collectors.Add(new KillCollector(demo, clock));
		}

		if (options.Includes(DemoCollectors.Blinds))
		{
			collectors.Add(new BlindCollector(demo, clock));
		}

		if (options.Includes(DemoCollectors.Grenades))
		{
			collectors.Add(new GrenadeCollector(demo, clock));
		}

		if (options.Includes(DemoCollectors.Positions))
		{
			collectors.Add(new PositionSampler(demo, clock));
		}

		return collectors;
	}

	#endregion
}
