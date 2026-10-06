#region Usings

using System.Text.RegularExpressions;
using HarnasHub.Application.Abstractions;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.Shared;

/// <summary>Object storage layout for match timelines. A timeline parsed before its match exists (the analyse step of
/// adding a result) is parked under <see cref="PendingPrefix"/> and moved to <see cref="MatchKey"/> once the result is
/// saved; unclaimed pending files are swept by a background job after <see cref="PendingRetention"/>.</summary>
public static partial class MatchTimelineStorage
{
	#region Public Fields

	/// <summary>Prefix of timelines parsed for a result that hasn't been saved yet.</summary>
	public const string PendingPrefix = "timelines/pending/";

	/// <summary>How long an unclaimed pending timeline is kept before cleanup.</summary>
	public static readonly TimeSpan PendingRetention = TimeSpan.FromHours(24);

	private const string ContentType = "application/gzip";

	#endregion

	#region Public Methods

	/// <summary>Final key of a match's timeline.</summary>
	public static string MatchKey(Guid matchResultId) => $"matches/{matchResultId}/timeline.json.gz";

	/// <summary>A fresh, unguessable pending key.</summary>
	public static string NewPendingKey() => $"{PendingPrefix}{Guid.NewGuid():N}.json.gz";

	/// <summary>Whether a client-supplied key is a well-formed pending key — the only kind a client may hand back,
	/// so a request can never make the server read or delete an arbitrary object.</summary>
	public static bool IsPendingKey(string? key) => key is not null && PendingKeyPattern().IsMatch(key);

	/// <summary>Serializes and uploads a timeline under <paramref name="objectKey"/>.</summary>
	public static async Task SaveAsync(
		IFileStorage fileStorage, string objectKey, DemoTimeline timeline, int parserVersion, CancellationToken cancellationToken)
	{
		TimelineMemoryCache.Forget(fileStorage, objectKey);

		using var buffer = new MemoryStream();
		await DemoTimelineSerializer.SerializeAsync(buffer, timeline, parserVersion, cancellationToken);
		buffer.Position = 0;
		await fileStorage.UploadAsync(objectKey, buffer, ContentType, cancellationToken);
	}

	/// <summary>Downloads and deserializes the timeline stored under <paramref name="objectKey"/>.</summary>
	public static Task<StoredDemoTimeline> LoadAsync(IFileStorage fileStorage, string objectKey, CancellationToken cancellationToken) =>
		TimelineMemoryCache.GetOrLoadAsync(fileStorage, objectKey, async () =>
		{
			await using var stream = await fileStorage.OpenReadAsync(objectKey, cancellationToken);
			return await DemoTimelineSerializer.DeserializeAsync(stream, cancellationToken);
		});

	#endregion

	#region Private Methods

	[GeneratedRegex("^timelines/pending/[0-9a-f]{32}\\.json\\.gz$")]
	private static partial Regex PendingKeyPattern();

	#endregion
}
