namespace HarnasHub.Application.Common.Notifications;

/// <summary>Helpers keeping Discord messages within the 2000-character limit without cutting a word, a line or the trailing link.</summary>
public static class DiscordMessage
{
	#region Public Fields

	/// <summary>Discord's hard limit for a single message.</summary>
	public const int MaxLength = 2000;

	#endregion

	#region Public Methods

	/// <summary>The message cut to <see cref="MaxLength"/> at a line or word boundary with a trailing "…"; shorter messages are returned unchanged.</summary>
	public static string Truncate(string message) => Truncate(message, MaxLength);

	/// <summary>Appends <paramref name="footer"/> (e.g. a link line; may be null) to <paramref name="body"/>, shortening only the body so the footer always survives.</summary>
	public static string WithFooter(string body, string? footer)
	{
		var footerLength = footer?.Length ?? 0;
		return Truncate(body, Math.Max(1, MaxLength - footerLength)) + footer;
	}

	#endregion

	#region Private Methods

	private static string Truncate(string text, int limit)
	{
		if (text.Length <= limit)
		{
			return text;
		}

		var end = Math.Max(1, limit - 1);
		if (char.IsHighSurrogate(text[end - 1]))
		{
			end--;
		}

		var cut = text.LastIndexOf('\n', end - 1, end);
		if (cut < end / 2)
		{
			cut = text.LastIndexOf(' ', end - 1, end);
		}

		if (cut < end / 2)
		{
			cut = end;
		}

		return text[..cut].TrimEnd() + "…";
	}

	#endregion
}
