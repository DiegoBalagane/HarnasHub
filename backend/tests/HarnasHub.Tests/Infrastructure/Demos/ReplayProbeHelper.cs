#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;
using HarnasHub.Application.Features.MatchAnalysis.Shared.Replay;
using HarnasHub.Infrastructure.Demos;
using Xunit;
using Xunit.Abstractions;

#endregion

namespace HarnasHub.Tests.Infrastructure.Demos;

/// <summary>Dev-only end-to-end probe of the 2D replay pipeline on a local demo: set <c>HARNASHUB_REPLAY_PROBE_DEMO</c> to a .dem
/// path and run <c>dotnet test --filter ReplayProbeHelper --logger "console;verbosity=detailed"</c>. It parses with the match
/// options, round-trips the timeline through the storage serializer and prints what the replay of a few rounds contains.
/// Without the variable it does nothing, so it never affects CI.</summary>
public class ReplayProbeHelper(ITestOutputHelper output)
{
	#region Public Methods

	/// <summary>Prints frame/player counts of the replay built from the demo named by the environment variable.</summary>
	[Fact]
	public async Task Print_replay_contents_of_a_local_demo()
	{
		var path = Environment.GetEnvironmentVariable("HARNASHUB_REPLAY_PROBE_DEMO");
		if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
		{
			return;
		}

		await using var stream = File.OpenRead(path);
		var parsed = await new DemoFileParser().ParseAsync(stream, DemoParseOptions.MatchAnalysis, CancellationToken.None);

		using var buffer = new MemoryStream();
		await DemoTimelineSerializer.SerializeAsync(buffer, parsed, DemoTimelineSerializer.CurrentParserVersion, CancellationToken.None);
		buffer.Position = 0;
		var timeline = (await DemoTimelineSerializer.DeserializeAsync(buffer, CancellationToken.None)).Timeline;

		output.WriteLine($"Mapa: {timeline.MapName}, rundy: {timeline.Rounds.Count}, tracki: {timeline.Positions.Count}, zapis: {buffer.Length / 1024} KB");
		var participants = RoundParticipants.Filter(timeline);
		var rawRoster = timeline.Rounds.SelectMany(r => r.TerroristSteamIds.Concat(r.CounterTerroristSteamIds)).Distinct().Count();
		var playingRoster = participants.Rounds.SelectMany(r => r.TerroristSteamIds.Concat(r.CounterTerroristSteamIds)).Distinct().Count();
		var dropped = timeline.Rounds.SelectMany(r => r.TerroristSteamIds.Concat(r.CounterTerroristSteamIds)).Distinct()
			.Except(participants.Rounds.SelectMany(r => r.TerroristSteamIds.Concat(r.CounterTerroristSteamIds))).ToList();
		output.WriteLine($"Skład w rundach: {rawRoster}, po filtrze uczestników: {playingRoster}, odrzuceni: [{string.Join(", ", dropped)}]");
		var firstRound = timeline.Rounds.FirstOrDefault();
		var ours = firstRound?.TerroristSteamIds ?? [];
		var theirs = firstRound?.CounterTerroristSteamIds ?? [];

		foreach (var round in timeline.Rounds.Take(3))
		{
			var replay = RoundReplayMapper.Build(timeline, round.Number, new ReplayPerspective(ours, theirs, "probe"));
			if (replay is null)
			{
				output.WriteLine($"Runda {round.Number}: brak replay");
				continue;
			}

			var playerCounts = replay.Frames.Select(f => f.Players.Count).ToList();
			output.WriteLine(
				$"Runda {round.Number}: status {replay.PositionsStatus}, klatek {replay.Frames.Count}, graczy {replay.Players.Count}, " +
				$"graczy w klatkach min/max {(playerCounts.Count == 0 ? "-" : $"{playerCounts.Min()}/{playerCounts.Max()}")}, granatów {replay.Grenades.Count}");
		}
	}

	#endregion
}
