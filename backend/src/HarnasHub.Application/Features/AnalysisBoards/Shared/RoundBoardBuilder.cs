#region Usings

using System.Text.Json;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.Shared.Replay;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Features.AnalysisBoards.Shared;

/// <summary>One point of a board stroke as a radar fraction (the board canvas' own format).</summary>
public record BoardPoint(float X, float Y);

/// <summary>One board stroke exactly as the board canvas stores it: colour, width in its 1000 px reference space, polyline.</summary>
public record BoardStroke(string Color, int Width, IReadOnlyList<BoardPoint> Points);

/// <summary>Pure snapshot of a replayed round at one second as analysis-board strokes. Boards hold freehand strokes only,
/// so everything is drawn the way the opponent-cluster boards are: a player is a small, thick circle (a filled dot) in
/// the side colour, a dead player a grey cross, a grenade its throw line plus a circle at the landing sized by type, and
/// a planted bomb a red square. Names can't be drawn — the title carries the context instead.</summary>
public static class RoundBoardBuilder
{
	#region Private Fields

	private const string CtColor = "#60a5fa";
	private const string TColor = "#f59e0b";
	private const string DeadColor = "#a3a3a3";
	private const string BombColor = "#dc2626";
	private const float DotRadius = 0.006f;
	private const float CrossHalf = 0.008f;
	private const int CircleSegments = 16;

	private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

	#endregion

	#region Public Methods

	/// <summary>Strokes for <paramref name="second"/> of the replay; empty when positions aren't available.</summary>
	public static IReadOnlyList<BoardStroke> Build(RoundReplayDto replay, int second)
	{
		var strokes = new List<BoardStroke>();
		if (replay.PositionsStatus != ReplayPositionsStatus.Available)
		{
			return strokes;
		}

		foreach (var grenade in replay.Grenades.Where(g => g.ThrowSecond <= second && second <= g.EndSecond))
		{
			strokes.AddRange(Grenade(grenade, second));
		}

		foreach (var kill in replay.Kills.Where(k => k.Second <= second && k.X is not null && k.Y is not null))
		{
			strokes.AddRange(Cross(kill.X!.Value, kill.Y!.Value, DeadColor));
		}

		if (replay.Bomb is { X: { } bx, Y: { } by } bomb && bomb.PlantSecond <= second)
		{
			strokes.Add(Square(bx, by, BombColor));
		}

		var frame = second >= 0 && second < replay.Frames.Count ? replay.Frames[second] : null;
		foreach (var state in frame?.Players ?? [])
		{
			var side = replay.Players[state.Player].Side;
			strokes.Add(new BoardStroke(side == MapSide.CT ? CtColor : TColor, 10, Circle(state.X, state.Y, DotRadius)));
		}

		return strokes;
	}

	/// <summary>The board title: "Runda N – mm:ss – vs X" (the opponent part omitted when unknown).</summary>
	public static string Title(RoundReplayDto replay, int second)
	{
		var time = $"{second / 60}:{second % 60:00}";
		var title = replay.OpponentName is { Length: > 0 } opponent
			? $"Runda {replay.RoundNumber} – {time} – vs {opponent}"
			: $"Runda {replay.RoundNumber} – {time}";
		return title.Length > 150 ? title[..150] : title;
	}

	/// <summary>Serializes strokes into the camelCase JSON the board canvas reads.</summary>
	public static string ToJson(IReadOnlyList<BoardStroke> strokes) => JsonSerializer.Serialize(strokes, JsonOptions);

	/// <summary>Circle radius drawn at a grenade's landing.</summary>
	public static float LandingRadius(DemoGrenadeType type) => type switch
	{
		DemoGrenadeType.Smoke => 0.025f,
		DemoGrenadeType.Molotov or DemoGrenadeType.Incendiary => 0.02f,
		_ => 0.012f
	};

	#endregion

	#region Private Methods

	private static IEnumerable<BoardStroke> Grenade(ReplayGrenadeDto grenade, int second)
	{
		if (grenade is not { LandX: { } lx, LandY: { } ly })
		{
			yield break;
		}

		var color = Color(grenade.Type);
		if (grenade.ThrowX is { } tx && grenade.ThrowY is { } ty)
		{
			yield return new BoardStroke(color, 2, [new BoardPoint(tx, ty), new BoardPoint(lx, ly)]);
		}

		if (second >= grenade.DetonateSecond)
		{
			yield return new BoardStroke(color, 4, Circle(lx, ly, LandingRadius(grenade.Type)));
		}
	}

	private static string Color(DemoGrenadeType type) => type switch
	{
		DemoGrenadeType.Smoke => "#d4d4d4",
		DemoGrenadeType.Flash => "#facc15",
		DemoGrenadeType.HighExplosive => "#ef4444",
		DemoGrenadeType.Molotov or DemoGrenadeType.Incendiary => "#f97316",
		_ => DeadColor
	};

	private static List<BoardPoint> Circle(float x, float y, float radius) =>
		Enumerable.Range(0, CircleSegments + 1)
			.Select(i => i * 2 * MathF.PI / CircleSegments)
			.Select(angle => new BoardPoint(Round(x + radius * MathF.Cos(angle)), Round(y + radius * MathF.Sin(angle))))
			.ToList();

	private static IEnumerable<BoardStroke> Cross(float x, float y, string color)
	{
		yield return new BoardStroke(color, 3, [new BoardPoint(x - CrossHalf, y - CrossHalf), new BoardPoint(x + CrossHalf, y + CrossHalf)]);
		yield return new BoardStroke(color, 3, [new BoardPoint(x - CrossHalf, y + CrossHalf), new BoardPoint(x + CrossHalf, y - CrossHalf)]);
	}

	private static BoardStroke Square(float x, float y, string color) => new(color, 4,
	[
		new BoardPoint(x - CrossHalf, y - CrossHalf),
		new BoardPoint(x + CrossHalf, y - CrossHalf),
		new BoardPoint(x + CrossHalf, y + CrossHalf),
		new BoardPoint(x - CrossHalf, y + CrossHalf),
		new BoardPoint(x - CrossHalf, y - CrossHalf)
	]);

	private static float Round(float value) => MathF.Round(value, 4);

	#endregion
}
