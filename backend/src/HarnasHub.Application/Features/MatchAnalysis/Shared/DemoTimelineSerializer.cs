#region Usings

using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using HarnasHub.Application.Abstractions;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.Shared;

/// <summary>A timeline as stored: the envelope versions say how to read it (<paramref name="FormatVersion"/>) and which
/// parser produced it (<paramref name="ParserVersion"/>), so old files can be detected and re-parsed later.</summary>
public record StoredDemoTimeline(int FormatVersion, int ParserVersion, DateTime CreatedAtUtc, DemoTimeline Timeline);

/// <summary>Gzip JSON (de)serialization of <see cref="DemoTimeline"/>. Enums are written as names so reordering an enum
/// can never silently change the meaning of a stored file.</summary>
public static class DemoTimelineSerializer
{
	#region Public Fields

	/// <summary>Version of the envelope/JSON layout; bump on a breaking change to how a file is read.</summary>
	public const int FormatVersion = 1;

	/// <summary>Version of what the collectors extract (3 = stage 3: economy, kills, bomb sites; 4 = stages 4–6: flash
	/// blinds, per-second positions in match timelines, grenade detonation times); bump whenever a collector changes its
	/// output so stale timelines can be found by <c>MatchDemoAnalysis.ParserVersion</c>. Older files stay readable: every
	/// section added since is an init-only property defaulting to empty/null, so a missing JSON key just means "not collected".</summary>
	public const int CurrentParserVersion = 4;

	#endregion

	#region Private Fields

	private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
	{
		Converters = { new JsonStringEnumConverter() },
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};

	#endregion

	#region Public Methods

	/// <summary>Writes the timeline wrapped in a current-version envelope as gzip JSON into <paramref name="destination"/>.</summary>
	public static async Task SerializeAsync(Stream destination, DemoTimeline timeline, int parserVersion, CancellationToken cancellationToken)
	{
		var envelope = new StoredDemoTimeline(FormatVersion, parserVersion, DateTime.UtcNow, timeline);

		await using var gzip = new GZipStream(destination, CompressionLevel.Optimal, leaveOpen: true);
		await JsonSerializer.SerializeAsync(gzip, envelope, Options, cancellationToken);
	}

	/// <summary>Reads a gzip JSON envelope; throws <see cref="InvalidDataException"/> for an empty or newer-format file.</summary>
	public static async Task<StoredDemoTimeline> DeserializeAsync(Stream source, CancellationToken cancellationToken)
	{
		await using var gzip = new GZipStream(source, CompressionMode.Decompress, leaveOpen: true);
		var envelope = await JsonSerializer.DeserializeAsync<StoredDemoTimeline>(gzip, Options, cancellationToken);

		if (envelope?.Timeline is null)
		{
			throw new InvalidDataException("Plik osi czasu meczu jest pusty.");
		}

		if (envelope.FormatVersion > FormatVersion)
		{
			throw new InvalidDataException($"Nieobsługiwana wersja pliku osi czasu: {envelope.FormatVersion}.");
		}

		return envelope;
	}

	#endregion
}
