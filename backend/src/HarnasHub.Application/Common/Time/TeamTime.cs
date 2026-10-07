#region Usings

using System.Globalization;

#endregion

namespace HarnasHub.Application.Common.Time;

/// <summary>Converts stored UTC instants to the team's wall-clock time (Europe/Warsaw, daylight saving included) for
/// text that leaves the app — Discord messages must show the hour the coach typed in, not UTC.</summary>
public static class TeamTime
{
	#region Private Fields

	// IANA id works on Linux and on Windows with .NET's ICU-based time zone support.
	private static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");

	#endregion

	#region Public Methods

	/// <summary>The local Polish time of a UTC instant (an unspecified kind is treated as UTC).</summary>
	public static DateTime ToLocal(DateTime utc) =>
		TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Zone);

	/// <summary>"dd.MM HH:mm" in Polish local time, e.g. "07.10 19:00".</summary>
	public static string ShortDateTime(DateTime utc) => ToLocal(utc).ToString("dd.MM HH:mm", CultureInfo.InvariantCulture);

	#endregion
}
