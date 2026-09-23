namespace HarnasHub.Application.Features.AnalysisBoards.Shared;

/// <summary>One saved drawing, ready to render — <paramref name="BackgroundImageUrl"/> is a freshly presigned,
/// time-limited GET URL resolved from <paramref name="BackgroundImageObjectKey"/> (null draws over the built-in
/// radar for <paramref name="MapName"/> instead). The raw object key is included too, alongside the resolved URL,
/// so an edit that doesn't touch the background can round-trip it unchanged instead of the client having to treat
/// "not touched" and "cleared" as the same thing. <paramref name="StrokesJson"/> is the raw freehand-stroke array,
/// left for the frontend to parse and replay onto its canvas rather than deserialized here.</summary>
public record AnalysisBoardDto(
	Guid Id,
	string MapName,
	string Title,
	string? BackgroundImageObjectKey,
	string? BackgroundImageUrl,
	string StrokesJson,
	DateTime UpdatedAtUtc);

/// <summary>A presigned upload target for a board's background screenshot — mirrors <c>PresignDemoUploadResultDto</c>.</summary>
public record PresignBoardImageUploadResultDto(string UploadUrl, string ObjectKey);
