#region Usings

using DemoFile;
using HarnasHub.Application.Abstractions;

#endregion

namespace HarnasHub.Infrastructure.Demos;

/// <summary>Implements <see cref="IDemoParser"/> against the DemoFile.Net library (github.com/saul/demofile-net) — the only
/// actively maintained C# parser for CS2's Source 2 demo format (the older CS:GO-era parsers don't read it).</summary>
public class DemoFileParser : IDemoParser
{
	#region Public Methods

	public async Task<DemoParseResult> ParseAsync(Stream demoStream, CancellationToken cancellationToken)
	{
		var demo = new CsDemoParser();
		var session = new DemoParseSession(demo);
		session.Subscribe();

		var reader = DemoFileReader.Create(demo, demoStream);
		await reader.ReadAllAsync(cancellationToken);

		// game_newmap only fires on a mid-session map *change*, never for the map a recording starts on — which
		// is every standalone demo — so the server info packet (present from the start) is the reliable source.
		var rawMapName = demo.ServerInfo?.MapName;
		var mapName = rawMapName is null ? null : MapCalibration.ParseMapName(rawMapName);

		return session.BuildResult(mapName);
	}

	#endregion
}
