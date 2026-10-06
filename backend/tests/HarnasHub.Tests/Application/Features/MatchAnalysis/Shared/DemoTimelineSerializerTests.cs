#region Usings

using System.IO.Compression;
using System.Text;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Application.Features.Tactics;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.MatchAnalysis.Shared;

public class DemoTimelineSerializerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_round_trip_every_timeline_section()
	{
		var timeline = MatchTimelineFactory.Timeline(
			[MatchTimelineFactory.Round(1, MapSide.T, plantSite: DemoBombSite.B)],
			[MatchTimelineFactory.Economy(1, 800, 850)],
			[MatchTimelineFactory.Kill(1, 1, 3, isOpening: true, victimPosition: new DemoPosition(1, 2, 3, 0.4f, 0.5f))]) with
		{
			Stats = new DemoParseResult(1, MapName.Mirage, [], [])
		};

		using var buffer = new MemoryStream();
		await DemoTimelineSerializer.SerializeAsync(buffer, timeline, 3, CancellationToken.None);
		buffer.Position = 0;
		var stored = await DemoTimelineSerializer.DeserializeAsync(buffer, CancellationToken.None);

		Assert.Equal(3, stored.ParserVersion);
		Assert.Equal(DemoTimelineSerializer.FormatVersion, stored.FormatVersion);
		Assert.Equal(MapName.Mirage, stored.Timeline.MapName);
		Assert.Equal(DemoBombSite.B, stored.Timeline.Rounds[0].BombPlant!.Site);
		Assert.Equal(DemoTimelineFactory.TeamA, stored.Timeline.Rounds[0].TerroristSteamIds);
		Assert.Equal(4, stored.Timeline.Economy[0].Players.Count);
		Assert.True(stored.Timeline.Kills[0].IsOpening);
		Assert.Equal(0.4f, stored.Timeline.Kills[0].Victim.Position!.RadarX);
		Assert.Equal(1, stored.Timeline.Stats!.RoundsPlayed);
	}

	[Fact]
	public async Task Should_round_trip_positions_and_grenade_detonation_times()
	{
		var grenade = DemoTimelineFactory.Grenade(1, 1) with { DetonationSecond = 12.5f };
		var timeline = DemoTimelineFactory.Timeline([DemoTimelineFactory.Round(1, MapSide.T)], [grenade]) with
		{
			Positions = [HarnasHub.Tests.Application.Features.OpponentReport.OpponentTimelineFactory.Track(1, 1, MapSide.T, (0.25f, 0.75f))]
		};

		using var buffer = new MemoryStream();
		await DemoTimelineSerializer.SerializeAsync(buffer, timeline, DemoTimelineSerializer.CurrentParserVersion, CancellationToken.None);
		buffer.Position = 0;
		var stored = await DemoTimelineSerializer.DeserializeAsync(buffer, CancellationToken.None);

		Assert.Equal(4, stored.ParserVersion);
		Assert.Equal(12.5f, stored.Timeline.Grenades[0].DetonationSecond);
		Assert.Equal(0.75f, stored.Timeline.Positions[0].At(3)!.RadarY);
	}

	[Fact]
	public async Task Should_read_a_version_3_file_without_the_newer_sections_as_empty()
	{
		const string json = """{"formatVersion":1,"parserVersion":3,"createdAtUtc":"2026-01-01T00:00:00Z","timeline":{"mapName":"Mirage","isRadarCalibrated":true,"rounds":[],"grenades":[{"id":1,"roundNumber":1,"type":"Smoke","throwerName":"x","secondsIntoRound":5,"throw":{"worldX":0,"worldY":0,"worldZ":0}}]}}""";
		using var buffer = new MemoryStream();
		await using (var gzip = new GZipStream(buffer, CompressionLevel.Fastest, leaveOpen: true))
		{
			await gzip.WriteAsync(Encoding.UTF8.GetBytes(json));
		}

		buffer.Position = 0;
		var stored = await DemoTimelineSerializer.DeserializeAsync(buffer, CancellationToken.None);

		Assert.Equal(3, stored.ParserVersion);
		Assert.Empty(stored.Timeline.Positions);
		Assert.Empty(stored.Timeline.Blinds);
		Assert.Null(stored.Timeline.Grenades[0].DetonationSecond);
	}

	[Fact]
	public async Task Should_reject_a_file_from_a_newer_format()
	{
		const string json = """{"formatVersion":99,"parserVersion":9,"createdAtUtc":"2026-01-01T00:00:00Z","timeline":{"rounds":[],"grenades":[],"isRadarCalibrated":false}}""";
		using var buffer = new MemoryStream();
		await using (var gzip = new GZipStream(buffer, CompressionLevel.Fastest, leaveOpen: true))
		{
			await gzip.WriteAsync(Encoding.UTF8.GetBytes(json));
		}

		buffer.Position = 0;

		await Assert.ThrowsAsync<InvalidDataException>(() => DemoTimelineSerializer.DeserializeAsync(buffer, CancellationToken.None));
	}

	[Fact]
	public void Should_accept_only_well_formed_pending_keys()
	{
		Assert.True(MatchTimelineStorage.IsPendingKey(MatchTimelineStorage.NewPendingKey()));
		Assert.False(MatchTimelineStorage.IsPendingKey("demos/0123456789abcdef0123456789abcdef"));
		Assert.False(MatchTimelineStorage.IsPendingKey("timelines/pending/../../matches/x.json.gz"));
		Assert.False(MatchTimelineStorage.IsPendingKey(null));
	}

	#endregion
}
